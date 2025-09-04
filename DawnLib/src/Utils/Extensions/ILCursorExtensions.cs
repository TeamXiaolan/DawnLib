using System.Reflection;
using Mono.Cecil.Cil;
using MonoMod.Cil;

namespace Dawn.Utils;
public static class ILCursorExtensions
{
    public static void EmitLdfld<T>(this ILCursor c, string fieldName) {
        var field = typeof(T).GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        c.Emit(OpCodes.Ldfld, field);
    }

    public static void EmitCallvirt<T>(this ILCursor c, string methodName)
    {
        var method = typeof(T).GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        c.Emit(OpCodes.Callvirt, method);
    }

    public static void EmitLdloc(this ILCursor c, int index)
    {
        c.Emit(OpCodes.Ldloc, index);
    }
}