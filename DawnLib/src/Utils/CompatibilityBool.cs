using System;

namespace Dawn.Utils;
public enum CompatibilityBool
{
    Never,
    Always,
    IfVersionMatches,
}

public static class CompatibilityBoolExtensions
{
    public static bool ShouldRunCompatibility(this CompatibilityBool value, string compatibilityVersion, Version currentModVersion)
    {
        return value switch
        {
            CompatibilityBool.Never => false,
            CompatibilityBool.Always => true,
            CompatibilityBool.IfVersionMatches => Version.Parse(compatibilityVersion) == currentModVersion, // support doing like 2.*
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),
        };
    }
}