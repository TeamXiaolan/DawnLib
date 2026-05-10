using System;
using System.Reflection;
using GameNetcodeStuff;

namespace Dawn.Utils;

public static class ReflectionUtils
{
    private static readonly Assembly VanillaAssembly = Assembly.GetAssembly(typeof(PlayerControllerB));
    public static Type GetVanillaType(string fullName)
    {
        return VanillaAssembly.GetType(fullName, throwOnError: true);
    }
}