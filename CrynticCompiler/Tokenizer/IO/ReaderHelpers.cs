using System.Reflection;
using System.Reflection.Emit;

namespace CrynticCompiler.Tokenizer.IO;

internal static class ReaderHelpers
{
    private static readonly Action<StreamReader, Stream, long> setPos;
    private static readonly Func<StreamReader, Stream, long> getPos;
    
    static ReaderHelpers()
    {
        FieldInfo charPosField =
            typeof(StreamReader).GetField("_charPos", BindingFlags.Instance | BindingFlags.NonPublic)!;
        
        FieldInfo charLenField =
            typeof(StreamReader).GetField("_charLen", BindingFlags.Instance | BindingFlags.NonPublic)!;

        PropertyInfo positionProperty =
            typeof(Stream).GetProperty("Position", BindingFlags.Instance | BindingFlags.Public)!;
        
        DynamicMethod getMethod = new("GetStreamReaderPosition",typeof(long), new [] {typeof(StreamReader), typeof(Stream) });
        
        ILGenerator getMethodGenerator = getMethod.GetILGenerator();
        
        getMethodGenerator.Emit(OpCodes.Ldarg_1);
        getMethodGenerator.Emit(OpCodes.Callvirt, positionProperty.GetMethod!);
        
        getMethodGenerator.Emit(OpCodes.Ldarg_0);
        getMethodGenerator.Emit(OpCodes.Ldfld, charLenField);
        
        getMethodGenerator.Emit(OpCodes.Sub);
        
        getMethodGenerator.Emit(OpCodes.Ldarg_0);
        getMethodGenerator.Emit(OpCodes.Ldfld, charPosField);
        
        getMethodGenerator.Emit(OpCodes.Add);
        
        getMethodGenerator.Emit(OpCodes.Ret);

        getPos = getMethod.CreateDelegate<Func<StreamReader, Stream, long>>();
        
        DynamicMethod setMethod = new("SetStreamReaderPosition",null, new [] {typeof(StreamReader), typeof(Stream), typeof(long)});
        
        ILGenerator setMethodGenerator = setMethod.GetILGenerator();
        
        setMethodGenerator.Emit(OpCodes.Ldarg_0);
        setMethodGenerator.Emit(OpCodes.Ldarg_1);
        
        setMethodGenerator.Emit(OpCodes.Call, getMethod);
        
        setMethodGenerator.Emit(OpCodes.Ldarg_2);
        
        setMethodGenerator.Emit(OpCodes.Sub);
        getMethodGenerator.Emit(OpCodes.Conv_I4);
        
        setMethodGenerator.Emit(OpCodes.Ldarg_0);
        setMethodGenerator.Emit(OpCodes.Ldfld, charPosField);
        
        setMethodGenerator.Emit(OpCodes.Add);
        LocalBuilder local = setMethodGenerator.DeclareLocal(typeof(int));
        setMethodGenerator.Emit(OpCodes.Stloc_0, local);
        
        setMethodGenerator.Emit(OpCodes.Ldarg_0);
        setMethodGenerator.Emit(OpCodes.Ldfld, charPosField);
        
        setMethodGenerator.Emit(OpCodes.Ldarg_0);
        setMethodGenerator.Emit(OpCodes.Ldfld, charLenField);

        Label label = setMethodGenerator.DefineLabel();
        setMethodGenerator.Emit(OpCodes.Blt_Un_S, label);
        
        setMethodGenerator.Emit(OpCodes.Ldarg_0);
        setMethodGenerator.Emit(OpCodes.Ldloc_0);
        setMethodGenerator.Emit(OpCodes.Stfld, charPosField);
        setMethodGenerator.Emit(OpCodes.Ret);
        
        setMethodGenerator.MarkLabel(label);
        
        setMethodGenerator.Emit(OpCodes.Ldarg_0);
        setMethodGenerator.Emit(OpCodes.Ldarg_0);
        setMethodGenerator.Emit(OpCodes.Ldfld, charLenField);
        setMethodGenerator.Emit(OpCodes.Stfld, charPosField);
        
        setMethodGenerator.Emit(OpCodes.Ret);

        setPos = setMethod.CreateDelegate<Action<StreamReader, Stream, long>>();
    }

    public static void SetPosition(StreamReader reader, long position) =>
        setPos(reader, reader.BaseStream, position);

    public static long GetPosition(StreamReader reader) =>
        getPos(reader, reader.BaseStream);
}