using System;
using System.Diagnostics.CodeAnalysis;
using CodeRebirthLib.Internal;
using CodeRebirthLib.Preloader;

namespace CodeRebirthLib;

public static class SelectableLevelExtensions
{
    public static NamespacedKey<CRMoonInfo> ToNamespacedKey(this SelectableLevel level)
    {
        if (!level.TryGetCRInfo(out CRMoonInfo? moonInfo))
        {
            Debuggers.Moons?.Log($"SelectableLevel {level} has no CRInfo");
            throw new Exception();
        }
        return moonInfo.TypedKey;
    }

    internal static bool TryGetCRInfo(this SelectableLevel level, [NotNullWhen(true)] out CRMoonInfo? moonInfo)
    {
        moonInfo = (CRMoonInfo)((ICRObject)level).CRInfo;
        return moonInfo != null;
    }

    internal static void SetCRInfo(this SelectableLevel level, CRMoonInfo moonInfo)
    {
        ((ICRObject)level).CRInfo = moonInfo;
    }
}
