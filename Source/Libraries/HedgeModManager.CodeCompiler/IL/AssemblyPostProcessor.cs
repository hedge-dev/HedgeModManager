namespace HedgeModManager.CodeCompiler.IL;
using System.Reflection;
using Mono.Cecil;
using Mono.Cecil.Cil;

public class AssemblyPostProcessor
{
    public byte[] Process(Stream assemblyStream)
    {
        var asmDef = AssemblyDefinition.ReadAssembly(assemblyStream, new ReaderParameters()
        {
            ReadSymbols = true,
            SymbolReaderProvider = new EmbeddedPortablePdbReaderProvider(),
            InMemory = true,
        });

        var modDef = asmDef.MainModule;

        foreach (var typeDef in modDef.Types)
        {
            if (typeDef.FullName == "System.Runtime.CompilerServices.Unsafe")
            {
                foreach (var methodDef in typeDef.Methods)
                {
                    UnsafeIntrinsicsProcessor.ProcessMethod(methodDef);
                }
            }
        }

        var outStream = new MemoryStream();
        asmDef.Write(outStream, new WriterParameters()
        {
            WriteSymbols = true,
            SymbolWriterProvider = new LocalSymbolWriterProvider() { Name = modDef.Name },
        });

        return outStream.ToArray();
    }

    public class LocalSymbolWriterProvider : ISymbolWriterProvider
    {
        public string Name { get; init; } = "Null";
        public ISymbolWriter GetSymbolWriter(ModuleDefinition module, string fileName)
        {
            // Workaround:
            // Ignore fileName parameter, an empty string gets passed by the writer
            // which when forwarded causes a crash. So we override it with a string of our own
            return new EmbeddedPortablePdbWriterProvider().GetSymbolWriter(module, Name);
        }

        public ISymbolWriter GetSymbolWriter(ModuleDefinition module, Stream symbolStream)
        {
            return new EmbeddedPortablePdbWriterProvider().GetSymbolWriter(module, symbolStream);
        }
    }
}