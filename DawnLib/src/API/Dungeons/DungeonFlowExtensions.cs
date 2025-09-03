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
        if (!dungeonFlow.TryGetCRInfo(out DawnDungeonInfo? dungeonInfo))
        {
            Debuggers.Dungeons?.Log($"DungeonFlow {dungeonFlow} has no CRInfo");
            throw new Exception();
        }
        return dungeonInfo.TypedKey;
    }

    internal static bool TryGetCRInfo(this DungeonFlow dungeonFlow, [NotNullWhen(true)] out DawnDungeonInfo? dungeonInfo)
    {
        object newObject = dungeonFlow;
        dungeonInfo = (DawnDungeonInfo)((ICRObject)newObject).CRInfo;
        return dungeonInfo != null;
    }

    internal static void SetCRInfo(this DungeonFlow dungeonFlow, DawnDungeonInfo dungeonInfo)
    {
        object newObject = dungeonFlow;
        ((ICRObject)newObject).CRInfo = dungeonInfo;
    }
}
