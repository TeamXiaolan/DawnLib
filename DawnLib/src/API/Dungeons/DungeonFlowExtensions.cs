using System.Diagnostics.CodeAnalysis;
using Dawn.Interfaces;
using DunGen.Graph;

namespace Dawn;

public static class DungeonFlowExtensions
{
    public static DawnDungeonInfo GetDawnInfo(this DungeonFlow dungeonFlow)
    {
        DawnDungeonInfo dungeonInfo = (DawnDungeonInfo)((IDawnObject)dungeonFlow).DawnInfo;
        return dungeonInfo;
    }

    public static bool TryGetDawnInfo(this DungeonFlow dungeonFlow, [NotNullWhen(true)] out DawnDungeonInfo? dungeonInfo)
    {
        dungeonInfo = (DawnDungeonInfo)((IDawnObject)dungeonFlow).DawnInfo;
        return dungeonInfo != null;
    }

    internal static bool HasDawnInfo(this DungeonFlow dungeonFlow)
    {
        return dungeonFlow.GetDawnInfo() != null;
    }

    internal static void SetDawnInfo(this DungeonFlow dungeonFlow, DawnDungeonInfo dungeonInfo)
    {
        ((IDawnObject)dungeonFlow).DawnInfo = dungeonInfo;
    }
}
