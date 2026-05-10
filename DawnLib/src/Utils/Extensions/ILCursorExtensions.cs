using System;
using System.Reflection;
using Mono.Cecil.Cil;
using MonoMod.Cil;

namespace Dawn.Utils;

public static class ILCursorExtensions
{
    public static void EmitLdfld(this ILCursor c, Type type, string fieldName)
    {
        var field = type.GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        c.Emit(OpCodes.Ldfld, field);
    }

    public static void EmitLdflda(this ILCursor c, Type type, string fieldName)
    {
        var field = type.GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        c.Emit(OpCodes.Ldflda, field);
    }

    public static void EmitLdfld<T>(this ILCursor c, string fieldName)
    {
        c.EmitLdfld(typeof(T), fieldName);
    }

    public static void EmitLdflda<T>(this ILCursor c, string fieldName)
    {
        c.EmitLdflda(typeof(T), fieldName);
    }

    public static void EmitVanillaLdfld(this ILCursor c, string typeName, string fieldName)
    {
        var field = ReflectionUtils.GetVanillaType(typeName).GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        c.Emit(OpCodes.Ldfld, field);
    }

    public static void EmitVanillaLdflda(this ILCursor c, string typeName, string fieldName)
    {
        var field = ReflectionUtils.GetVanillaType(typeName).GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        c.Emit(OpCodes.Ldflda, field);
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