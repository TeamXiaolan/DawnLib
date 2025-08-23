using System;
using System.Diagnostics.CodeAnalysis;
using CodeRebirthLib.Internal;
using CodeRebirthLib.Preloader;
using DunGen.Graph;

namespace CodeRebirthLib;

public static class DungeonFlowExtensions
{
    public static NamespacedKey<CRDungeonInfo> ToNamespacedKey(this DungeonFlow dungeonFlow)
    {
        if (!dungeonFlow.TryGetCRInfo(out CRDungeonInfo? dungeonInfo))
        {
            Debuggers.Dungeons?.Log($"DungeonFlow {dungeonFlow} has no CRInfo");
            throw new Exception();
        }
        return dungeonInfo.TypedKey;
    }

    internal static bool TryGetCRInfo(this DungeonFlow dungeonFlow, [NotNullWhen(true)] out CRDungeonInfo? dungeonInfo)
    {
        object newObject = dungeonFlow;
        dungeonInfo = (CRDungeonInfo)((ICRObject)newObject).CRInfo;
        return dungeonInfo != null;
    }

    internal static void SetCRInfo(this DungeonFlow dungeonFlow, CRDungeonInfo dungeonInfo)
    {
        object newObject = dungeonFlow;
        ((ICRObject)newObject).CRInfo = dungeonInfo;
    }
}
