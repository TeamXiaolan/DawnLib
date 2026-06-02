using System.Diagnostics.CodeAnalysis;
using Dawn.Interfaces;

namespace Dawn;

public static class SelectableLevelExtensions
{
    public static DawnMoonInfo GetDawnInfo(this SelectableLevel selectableLevel)
    {
        DawnMoonInfo moonInfo = (DawnMoonInfo)((IDawnObject)selectableLevel).DawnInfo;
        return moonInfo;
    }

    public static bool TryGetDawnInfo(this SelectableLevel selectableLevel, [NotNullWhen(true)] out DawnMoonInfo? moonInfo)
    {
        moonInfo = (DawnMoonInfo)((IDawnObject)selectableLevel).DawnInfo;
        return moonInfo != null;
    }

    internal static bool HasDawnInfo(this SelectableLevel selectableLevel)
    {
        return selectableLevel.GetDawnInfo() != null;
    }

    internal static void SetDawnInfo(this SelectableLevel level, DawnMoonInfo moonInfo)
    {
        ((IDawnObject)level).DawnInfo = moonInfo;
    }
}
