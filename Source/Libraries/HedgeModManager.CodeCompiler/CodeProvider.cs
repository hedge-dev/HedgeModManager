namespace HedgeModManager.CodeCompiler;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using System.IO.Compression;
using Properties;
using Foundation;
using Diagnostics;
using PreProcessor;
using IL;

public class CodeProvider
{
    private static object mLockContext = new object();

    public static MethodDeclarationSyntax LoaderExecutableMethod =
        SyntaxFactory.MethodDeclaration(SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.BoolKeyword)), "IsLoaderExecutable")
            .WithExpressionBody(SyntaxFactory.ArrowExpressionClause(SyntaxFactory.LiteralExpression(SyntaxKind.TrueLiteralExpression)))
            .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.StaticKeyword)));

    public static SyntaxTree[] PredefinedClasses =
    {
        CSharpSyntaxTree.ParseText(Resources.MemoryService),
        CSharpSyntaxTree.ParseText(Resources.Keys)
    };

    public static AssemblyPostProcessor PostProcessor = new();

    public static void TryLoadRoslyn()
    {
        new Thread(() =>
        {
            try
            {
                CompileCodes(Array.Empty<CSharpCode>(), Stream.Null);
            }
            catch { }
        }).Start();
    }

    public static Task<Report> CompileCodes<TCollection>(TCollection sources, string assemblyPath, IIncludeResolver? includeResolver = null, params string[] loadsPaths) where TCollection : IEnumerable<CSharpCode>
    {
        lock (mLockContext)
        {
            using var stream = File.Create(assemblyPath);
            return CompileCodes(sources, stream, includeResolver, loadsPaths);
        }
    }

    public static Task<Report> CompileCodes<TCollection>(TCollection sources, Stream resultStream,
        IIncludeResolver? includeResolver = null, params string[] loadPaths) where TCollection : IEnumerable<CSharpCode>
    {
        return CompileCodes(new CompilerOptions<TCollection>(sources)
        {
            AssemblyLookupPaths = loadPaths,
            IncludeResolver = includeResolver,
            OutputStream = resultStream,
            IncludeAllSources = false
        });
    }

    public static Task<Report> CompileCodes<TCollection>(in CompilerOptions<TCollection> compileOptions)
        where TCollection : IEnumerable<CSharpCode>
    {
        var sources = compileOptions.Sources;
        var includeResolver = compileOptions.IncludeResolver;
        var loadPaths = compileOptions.AssemblyLookupPaths;

        lock (mLockContext)
        {
            var report = new Report();
            var options = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true);
            var trees = new List<SyntaxTree>();
            var newLibs = new HashSet<string>();
            var loads = GetLoadAssemblies(sources, includeResolver, loadPaths);
            var resultStream = compileOptions.OutputStream;

            // We don't have to concern ourselves with resolving references if everything is compiled
            if (compileOptions.IncludeAllSources)
            {
                foreach (var source in sources)
                {
                    trees.Add(source.CreateSyntaxTree(includeResolver));
                }
            }
            else
            {
                foreach (var source in sources)
                {
                    if (!source.IsExecutable())
                    {
                        continue;
                    }

                    trees.Add(source.CreateSyntaxTree(includeResolver));

                    foreach (string reference in source.GetReferences())
                    {
                        newLibs.Add($"{source.Name}\x00{reference}");
                    }

                    foreach (string reference in source.GetImports())
                    {
                        newLibs.Add($"{source.Name}\x00{reference}");
                    }
                }
            }

            var libs = new HashSet<string>();
            while (newLibs.Count != 0)
            {
                var addedLibs = new List<string>(newLibs.Count);
                var hasError = false;
                foreach (string libIter in newLibs)
                {
                    var lib = libIter;
                    var dividerIndex = lib.IndexOf('\x00');
                    var sourceRef = string.Empty;

                    if (dividerIndex != -1)
                    {
                        sourceRef = lib.Substring(0, dividerIndex);
                        lib = lib.Substring(dividerIndex + 1);
                    }

                    if (libs.Contains(lib))
                    {
                        continue;
                    }

                    var libSource = sources.FirstOrDefault(x => x.Name == lib || x.ID == lib);
                    if (libSource == null)
                    {
                        report.Error(sourceRef, $"Unable to find dependency library '{lib}'");
                        hasError = true;
                        continue;
                    }

                    trees.Add(libSource.CreateSyntaxTree(includeResolver));
                    libs.Add(lib);

                    foreach (string reference in libSource.GetReferences())
                    {
                        addedLibs.Add(reference);
                    }

                    foreach (string reference in libSource.GetImports())
                    {
                        addedLibs.Add(reference);
                    }
                }

                if (hasError)
                {
                    return Task.FromResult(report);
                }

                newLibs.Clear();
                newLibs.UnionWith(addedLibs);
            }

            loads.Add(StaticAssemblyResolver.Resolve("mscorlib.dll")!);
            loads.Add(StaticAssemblyResolver.Resolve("System.dll")!);
            loads.Add(StaticAssemblyResolver.Resolve("System.Core.dll")!);
            loads.Add(StaticAssemblyResolver.Resolve("Microsoft.CSharp.dll")!);

            var compiler = CSharpCompilation.Create("HMMCodes", trees, loads, options)
                .AddSyntaxTrees(PredefinedClasses);

            var memStream = new MemoryStream();
            var result = compiler.Emit(memStream);
            memStream.Position = 0;

            if (result.Success)
            {
                var outData = PostProcessor.Process(memStream);
                resultStream.Write(outData, 0, outData.Length);
            }

            if (!result.Success)
            {
                foreach (var diagnostic in result.Diagnostics)
                {
                    var path = diagnostic.Location.SourceTree?.FilePath ?? string.Empty;
                    var line = diagnostic.Location.GetLineSpan();
                    var message =
                        $"@({line.StartLinePosition.Line + 1},{line.StartLinePosition.Character}) {diagnostic.Descriptor.Id}: {diagnostic.GetMessage()}";

                    switch (diagnostic.Severity)
                    {
                        case DiagnosticSeverity.Info:
                            report.Information(path, message);
                            break;

                        case DiagnosticSeverity.Warning:
                            report.Warning(path, message);
                            break;

                        case DiagnosticSeverity.Error:
                            report.Error(path, message);
                            break;
                    }
                }
            }

            return Task.FromResult(report);
        }
    }

    public static List<MetadataReference> GetLoadAssemblies<TCollection>(TCollection sources, IIncludeResolver? includeResolver = null, IEnumerable<string>? lookupPaths = null) where TCollection : IEnumerable<CSharpCode>
    {
        lookupPaths ??= Array.Empty<string>();
        var meta = new List<MetadataReference>();
        
        foreach (var source in sources)
        {
            foreach (var load in source.ParseSyntaxTree(includeResolver).PreprocessorDirectives.Where(x => x.Kind == SyntaxTokenKind.LoadDirectiveTrivia))
            {
                var value = load.Value.ToString();
                var staticRef = StaticAssemblyResolver.Resolve(value);
                if (staticRef != null)
                {
                    meta.Add(staticRef);
                    continue;
                }

                foreach (var lookupPath in lookupPaths)
                {
                    var path = Path.Combine(lookupPath, value);
                    if (File.Exists(path))
                    {
                        meta.Add(MetadataReference.CreateFromFile(path));
                    }
                }
            }
        }

        return meta;
    }

    private static class StaticAssemblyResolver
    {
        private static ZipArchive Archive { get; }
        private static Dictionary<string, MetadataReference?> ReferenceCache { get; } = new(StringComparer.InvariantCultureIgnoreCase);
        static StaticAssemblyResolver()
        {
            Archive = new ZipArchive(new MemoryStream(Resources.ReferenceAssemblies));
        }

        public static MetadataReference? Resolve(string name)
        {
            if (ReferenceCache.TryGetValue(name, out var reference))
            {
                return reference;
            }

            var entry = Archive.GetEntry(name);
            if (entry == null)
            {
                ReferenceCache.Add(name, null);
                return null;
            }

            var memStream = new MemoryStream();
            using var stream = entry.Open();
            stream.CopyTo(memStream);

            // Can't use CreateFromStream for some reason
            var metaRef = AssemblyMetadata.CreateFromImage(memStream.GetBuffer()).GetReference(filePath: $"res://{name}");
            ReferenceCache.Add(name, metaRef);
            return metaRef;
        }
    }
}

public class OptionalColonRewriter : CSharpSyntaxRewriter
{
    public override SyntaxToken VisitToken(SyntaxToken token)
    {
        if (token.IsKind(SyntaxKind.SemicolonToken) && token.IsMissing)
        {
            return SyntaxFactory.Token(SyntaxKind.SemicolonToken);
        }
        return base.VisitToken(token);
    }
}