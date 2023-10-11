namespace HedgeModManager.CodeCompiler.IL;
using Mono.Cecil.Cil;
using Mono.Cecil;

public static class AssemblyHelpers
{
    public static void SetOpCodes(this MethodDefinition method, params OpCode[] opcodes)
    {
        method.Body = new MethodBody(method);
        var processor = method.Body.GetILProcessor();
        foreach (var op in opcodes)
        {
            processor.Emit(op);
        }
    }
}