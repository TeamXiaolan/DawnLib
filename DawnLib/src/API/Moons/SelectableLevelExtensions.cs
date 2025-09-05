using System;
using System.Diagnostics.CodeAnalysis;
using Dawn.Internal;
using Dawn.Preloader.Interfaces;

namespace Dawn;

public static class SelectableLevelExtensions
{
    public static NamespacedKey<DawnMoonInfo> ToNamespacedKey(this SelectableLevel level)
    {
        if (!level.TryGetDawnInfo(out DawnMoonInfo? moonInfo))
        {
            Debuggers.Moons?.Log($"SelectableLevel {level} has no CRInfo");
            throw new Exception();
        }
        return moonInfo.TypedKey;
    }

    internal static bool TryGetDawnInfo(this SelectableLevel level, [NotNullWhen(true)] out DawnMoonInfo? moonInfo)
    {
        moonInfo = (DawnMoonInfo)((IDawnObject)level).DawnInfo;
        return moonInfo != null;
    }

    internal static DawnMoonInfo GetDawnInfo(this SelectableLevel level)
    {
        return (DawnMoonInfo)((IDawnObject)level).DawnInfo;
    }

    internal static void SetDawnInfo(this SelectableLevel level, DawnMoonInfo moonInfo)
    {
        ((IDawnObject)level).DawnInfo = moonInfo;
    }
}
