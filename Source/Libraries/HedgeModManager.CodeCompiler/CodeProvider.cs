namespace HedgeModManager.CodeCompiler;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using System.IO.Compression;
using Properties;
using Foundation;
using Diagnostics;
using PreProcessor;

public class CodeProvider
{
    private static object mLockContext = new object();

    public static MethodDeclarationSyntax LoaderExecutableMethod =
        SyntaxFactory.MethodDeclaration(SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.BoolKeyword)), "IsLoaderExecutable")
            .WithExpressionBody(SyntaxFactory.ArrowExpressionClause(SyntaxFactory.LiteralExpression(SyntaxKind.TrueLiteralExpression)))
            .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.StaticKeyword)));
        
    public static UsingDirectiveSyntax[] PredefinedUsingDirectives =
    {
        SyntaxFactory.UsingDirective(SyntaxFactory.IdentifierName("System")),
        SyntaxFactory.UsingDirective(SyntaxFactory.IdentifierName("HMMCodes")),
        SyntaxFactory.UsingDirective(
            SyntaxFactory.Token(SyntaxKind.UsingKeyword),
            SyntaxFactory.Token(SyntaxKind.StaticKeyword), null,
            SyntaxFactory.QualifiedName(SyntaxFactory.IdentifierName("HMMCodes"), SyntaxFactory.IdentifierName("MemoryService")), SyntaxFactory.Token(SyntaxKind.SemicolonToken)),
    };

    public static SyntaxTree[] PredefinedClasses =
    {
        CSharpSyntaxTree.ParseText(Resources.MemoryService),
        CSharpSyntaxTree.ParseText(Resources.Keys)
    };

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

    public static Task<Report> CompileCodes<TCollection>(TCollection sources, Stream resultStream, IIncludeResolver? includeResolver = null, params string[] loadPaths) where TCollection : IEnumerable<CSharpCode>
    {
        lock (mLockContext)
        {
            var options = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true);
            var trees = new List<SyntaxTree>();
            var newLibs = new HashSet<string>();
            var loads = GetLoadAssemblies(sources, includeResolver, loadPaths);

            foreach (var source in sources)
            {
                if (!source.IsExecutable())
                {
                    continue;
                }

                trees.Add(source.CreateSyntaxTree(includeResolver));

                foreach (string reference in source.GetReferences())
                {
                    newLibs.Add(reference);
                }

                foreach (string reference in source.GetImports())
                {
                    newLibs.Add(reference);
                }
            }

            var libs = new HashSet<string>();
            while (newLibs.Count != 0)
            {
                var addedLibs = new List<string>(newLibs.Count);
                foreach (string lib in newLibs)
                {
                    if (libs.Contains(lib))
                    {
                        continue;
                    }

                    var libSource = sources.FirstOrDefault(x => x.Name == lib);
                    if (libSource == null)
                    {
                        throw new Exception($"Unable to find dependency library {lib}");
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

                newLibs.Clear();
                newLibs.UnionWith(addedLibs);
            }
            
            loads.Add(StaticAssemblyResolver.Resolve("mscorlib.dll")!);
            loads.Add(StaticAssemblyResolver.Resolve("System.dll")!);
            loads.Add(StaticAssemblyResolver.Resolve("System.Core.dll")!);
            loads.Add(StaticAssemblyResolver.Resolve("Microsoft.CSharp.dll")!);

            var compiler = CSharpCompilation.Create("HMMCodes", trees, loads, options).AddSyntaxTrees(PredefinedClasses);

            var report = new Report();
            var result = compiler.Emit(resultStream);
            if (!result.Success)
            {
                foreach (var diagnostic in result.Diagnostics)
                {
                    var path = diagnostic.Location.SourceTree?.FilePath ?? string.Empty;
                    switch (diagnostic.Severity)
                    {
                        case DiagnosticSeverity.Info:
                            report.Information(path, diagnostic.GetMessage());
                            break;

                        case DiagnosticSeverity.Warning:
                            report.Warning(path, diagnostic.GetMessage());
                            break;

                        case DiagnosticSeverity.Error:
                            report.Error(path, diagnostic.GetMessage());
                            break;
                    }
                }
            }

            return Task.FromResult(report);
        }
    }

    public static List<MetadataReference> GetLoadAssemblies<TCollection>(TCollection sources, IIncludeResolver? includeResolver = null, params string[] lookupPaths) where TCollection : IEnumerable<CSharpCode>
    {
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
                        continue;
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