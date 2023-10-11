namespace HedgeModManager.CodeCompiler.IL;
using Mono.Cecil;

public class AssemblyPostProcessor
{
    public byte[] Process(Stream assemblyStream)
    {
        var asmDef = AssemblyDefinition.ReadAssembly(assemblyStream);
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
        asmDef.Write(outStream);
        return outStream.ToArray();
    }
}