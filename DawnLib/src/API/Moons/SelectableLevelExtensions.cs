using Dawn.Interfaces;

namespace Dawn;

public static class SelectableLevelExtensions
{
    public static DawnMoonInfo GetDawnInfo(this SelectableLevel selectableLevel)
    {
        DawnMoonInfo moonInfo = (DawnMoonInfo)((IDawnObject)selectableLevel).DawnInfo;
        return moonInfo;
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
