using System;
using System.Diagnostics.CodeAnalysis;
using Dawn.Internal;
using Dawn.Preloader.Interfaces;

namespace Dawn;

public static class SelectableLevelExtensions
{
    public static NamespacedKey<DawnMoonInfo> ToNamespacedKey(this SelectableLevel level)
    {
        if (!level.TryGetCRInfo(out DawnMoonInfo? moonInfo))
        {
            Debuggers.Moons?.Log($"SelectableLevel {level} has no CRInfo");
            throw new Exception();
        }
        return moonInfo.TypedKey;
    }

    internal static bool TryGetCRInfo(this SelectableLevel level, [NotNullWhen(true)] out DawnMoonInfo? moonInfo)
    {
        moonInfo = (DawnMoonInfo)((ICRObject)level).CRInfo;
        return moonInfo != null;
    }

    internal static void SetCRInfo(this SelectableLevel level, DawnMoonInfo moonInfo)
    {
        ((ICRObject)level).CRInfo = moonInfo;
    }
}
