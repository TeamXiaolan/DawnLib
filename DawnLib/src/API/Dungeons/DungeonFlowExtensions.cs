using System;
using System.Diagnostics.CodeAnalysis;
using Dawn.Internal;
using Dawn.Preloader.Interfaces;
using DunGen.Graph;

namespace Dawn;

public static class DungeonFlowExtensions
{
    public static NamespacedKey<DawnDungeonInfo> ToNamespacedKey(this DungeonFlow dungeonFlow)
    {
        if (!dungeonFlow.TryGetDawnInfo(out DawnDungeonInfo? dungeonInfo))
        {
            Debuggers.Dungeons?.Log($"DungeonFlow {dungeonFlow} has no CRInfo");
            throw new Exception();
        }
        return dungeonInfo.TypedKey;
    }

    internal static bool TryGetDawnInfo(this DungeonFlow dungeonFlow, [NotNullWhen(true)] out DawnDungeonInfo? dungeonInfo)
    {
        object newObject = dungeonFlow;
        dungeonInfo = (DawnDungeonInfo)((IDawnObject)newObject).DawnInfo;
        return dungeonInfo != null;
    }

    internal static void SetDawnInfo(this DungeonFlow dungeonFlow, DawnDungeonInfo dungeonInfo)
    {
        object newObject = dungeonFlow;
        ((IDawnObject)newObject).DawnInfo = dungeonInfo;
    }
}
