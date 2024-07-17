using System.Reflection;
using System.Reflection.Emit;

namespace CrynticCompiler.Extensions;

public class ReflectionExtensions
{
    public static Func<TIn, TOut> CreateFieldGetterDelegate<TIn, TOut>(string name, BindingFlags flags)
    {
        FieldInfo? field = typeof(TIn).GetField(name, flags);
        if (field is null)
        {
            throw new ArgumentException($"name was not found in the definition for {nameof(TIn)}", nameof(name));
        }

        if (field.FieldType != typeof(TOut))
        {
            throw new ArgumentException("typeof TOut does not match the field type");
        }

        DynamicMethod method = new("Get_" + name, typeof(TOut), new[] { typeof(TIn) });

        ILGenerator generator = method.GetILGenerator(8);
        
        generator.Emit(OpCodes.Ldarg_0);
        generator.Emit(OpCodes.Ldfld, field);
        generator.Emit(OpCodes.Ret);

        return method.CreateDelegate<Func<TIn, TOut>>();
    }
}