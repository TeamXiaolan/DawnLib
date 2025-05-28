using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BepInEx;
using CodeRebirthLib.ContentManagement;
using CodeRebirthLib.Extensions;

namespace CodeRebirthLib;
public static class CodeRebirthLib
{
    public static void RegisterContentHandlers(Assembly assembly)
    {
        IEnumerable<Type> contentHandlers = assembly.GetLoadableTypes().Where(x =>
            x.BaseType != null
            && x.BaseType.IsGenericType
            && x.BaseType.GetGenericTypeDefinition() == typeof(ContentHandler<>)
        );

        foreach (Type type in contentHandlers)
        {
            type.GetConstructor([]).Invoke([]);
        }
    }
}