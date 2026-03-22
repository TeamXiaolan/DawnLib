using System.Diagnostics.CodeAnalysis;
using Dawn.Interfaces;
using DunGen.Graph;

namespace Dawn;

public static class DungeonFlowExtensions
{
    public static DawnDungeonInfo GetDawnInfo(this DungeonFlow dungeonFlow)
    {
        object newObject = dungeonFlow;
        DawnDungeonInfo dungeonInfo = (DawnDungeonInfo)((IDawnObject)newObject).DawnInfo;
        return dungeonInfo;
    }

    public static bool TryGetDawnInfo(this DungeonFlow dungeonFlow, [NotNullWhen(true)] out DawnDungeonInfo? dungeonInfo)
    {
        object newObject = dungeonFlow;
        dungeonInfo = (DawnDungeonInfo)((IDawnObject)newObject).DawnInfo;
        return dungeonInfo != null;
    }

    internal static bool HasDawnInfo(this DungeonFlow dungeonFlow)
    {
        return dungeonFlow.GetDawnInfo() != null;
    }

    internal static void SetDawnInfo(this DungeonFlow dungeonFlow, DawnDungeonInfo dungeonInfo)
    {
        object newObject = dungeonFlow;
        ((IDawnObject)newObject).DawnInfo = dungeonInfo;
    }
}
