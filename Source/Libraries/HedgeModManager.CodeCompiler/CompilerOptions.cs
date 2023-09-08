namespace HedgeModManager.CodeCompiler;
using PreProcessor;

public struct CompilerOptions<TCollection> where TCollection : IEnumerable<CSharpCode>
{
    public TCollection Sources { get; set; }
    public Stream OutputStream { get; set; }
    public IIncludeResolver? IncludeResolver { get; set; }
    public IEnumerable<string>? AssemblyLookupPaths { get; set; }

    /// <summary>
    /// Specifies if in-executable codes like libraries get compiled even when unused
    /// </summary>
    public bool IncludeAllSources { get; set; }

    public CompilerOptions(TCollection sources)
    {
        Sources = sources;
        OutputStream = Stream.Null;
        AssemblyLookupPaths = Array.Empty<string>();
    }
}