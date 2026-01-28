using System;

namespace Dawn.Utils;
public enum CompatibilityBool
{
    Never,
    Always,
    IfVersionMatches,
    IfMajorVersionMatches,
}

public static class CompatibilityBoolExtensions
{
    public static bool ShouldRunCompatibility(this CompatibilityBool value, string compatibilityVersion, Version currentModVersion)
    {
        return value switch
        {
            CompatibilityBool.Never => false,
            CompatibilityBool.Always => true,
            CompatibilityBool.IfVersionMatches => Version.Parse(compatibilityVersion) == currentModVersion,
            CompatibilityBool.IfMajorVersionMatches => Version.Parse(compatibilityVersion).Major == currentModVersion.Major,
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),
        };
    }
}