namespace HedgeModManager.CodeCompiler.IL;
using Mono.Cecil.Cil;
using Mono.Cecil;

public static class UnsafeIntrinsicsProcessor
{
    public static void ProcessMethod(MethodDefinition method)
    {
        switch (method.Name)
        {
            case "AsPointer":
                method.SetOpCodes(OpCodes.Ldarg_0, OpCodes.Conv_U, OpCodes.Ret);
                break;

            case "SkipInit":
                method.SetOpCodes(OpCodes.Ret);
                break;

            case "AreSame":
                method.SetOpCodes(OpCodes.Ldarg_0, OpCodes.Ldarg_1, OpCodes.Ceq, OpCodes.Ret);
                break;

            case "InitBlock":
                method.EmitInitBlock(false);
                break;

            case "InitBlockUnaligned":
                method.EmitInitBlock(false);
                break;

            case "SizeOf":
                method.EmitSizeOf();
                break;

            case "As":
            case "AsRef":
                method.SetOpCodes(OpCodes.Ldarg_0, OpCodes.Ret);
                break;

            case "Add":
                method.EmitAdd();
                break;

            case "Read":
                method.EmitRead(false);
                break;

            case "ReadUnaligned":
                method.EmitRead(true);
                break;

            case "Copy":
                method.EmitCopy();
                break;

            case "CopyBlock":
                method.EmitCopyBlock(false);
                break;

            case "CopyBlockUnaligned":
                method.EmitCopyBlock(true);
                break;

        }
    }

    private static void EmitAdd(this MethodDefinition method)
    {
        var body = new MethodBody(method);
        method.Body = body;

        var processor = body.GetILProcessor();
        var type = method.GenericParameters.First();

        processor.Emit(OpCodes.Ldarg_0);
        processor.Emit(OpCodes.Ldarg_1);
        processor.Emit(OpCodes.Sizeof, type);
        processor.Emit(OpCodes.Mul);
        processor.Emit(OpCodes.Add);
        processor.Emit(OpCodes.Ret);
    }

    private static void EmitSizeOf(this MethodDefinition method)
    {
        var body = new MethodBody(method);
        method.Body = body;

        var processor = body.GetILProcessor();
        var type = method.GenericParameters.First();
        processor.Emit(OpCodes.Sizeof, type);
        processor.Emit(OpCodes.Ret);
    }

    private static void EmitRead(this MethodDefinition method, bool unaligned)
    {
        var body = new MethodBody(method);
        method.Body = body;

        var processor = body.GetILProcessor();
        processor.Emit(OpCodes.Ldarg_0);

        if (unaligned)
        {
            processor.Emit(OpCodes.Unaligned, (byte)1);
        }

        var type = method.GenericParameters.First();
        processor.Emit(OpCodes.Ldobj, type);
        processor.Emit(OpCodes.Ret);
    }

    private static void EmitCopy(this MethodDefinition method)
    {
        var body = new MethodBody(method);
        method.Body = body;

        var processor = body.GetILProcessor();
        processor.Emit(OpCodes.Ldarg_0);
        processor.Emit(OpCodes.Ldarg_1);

        var type = method.GenericParameters.First();
        processor.Emit(OpCodes.Ldobj, type);
        processor.Emit(OpCodes.Stobj, type);
        processor.Emit(OpCodes.Ret);
    }

    private static void EmitInitBlock(this MethodDefinition method, bool unaligned)
    {
        var body = new MethodBody(method);
        method.Body = body;

        var processor = body.GetILProcessor();

        processor.Emit(OpCodes.Ldarg_0);
        processor.Emit(OpCodes.Ldarg_1);
        processor.Emit(OpCodes.Ldarg_2);
        if (unaligned)
        {
            processor.Emit(OpCodes.Unaligned, (byte)1);
        }
        processor.Emit(OpCodes.Initblk);
        processor.Emit(OpCodes.Ret);
    }

    private static void EmitCopyBlock(this MethodDefinition method, bool unaligned)
    {
        var body = new MethodBody(method);
        method.Body = body;

        var processor = body.GetILProcessor();

        processor.Emit(OpCodes.Ldarg_0);
        processor.Emit(OpCodes.Ldarg_1);
        processor.Emit(OpCodes.Ldarg_2);
        if (unaligned)
        {
            processor.Emit(OpCodes.Unaligned, (byte)1);
        }
        processor.Emit(OpCodes.Cpblk);
        processor.Emit(OpCodes.Ret);
    }
}