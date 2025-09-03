using System;
using System.Diagnostics.CodeAnalysis;
using Dawn.Internal;
using Dawn.Preloader.Interfaces;
using DunGen;

namespace Dawn;

public static class DungeonArchetypeExtensions
{
    public static NamespacedKey<DawnArchetypeInfo> ToNamespacedKey(this DungeonArchetype archetype)
    {
        if (!archetype.TryGetDawnInfo(out DawnArchetypeInfo? tileSetInfo))
        {
            Debuggers.Dungeons?.Log($"Archetype {archetype} has no CRInfo");
            throw new Exception();
        }
        return tileSetInfo.TypedKey;
    }

    internal static bool TryGetDawnInfo(this DungeonArchetype archetype, [NotNullWhen(true)] out DawnArchetypeInfo? tileSetInfo)
    {
        object newObject = archetype;
        tileSetInfo = (DawnArchetypeInfo)((IDawnObject)newObject).DawnInfo;
        return tileSetInfo != null;
    }

    internal static void SetDawnInfo(this DungeonArchetype archetype, DawnArchetypeInfo tileSetInfo)
    {
        object newObject = archetype;
        ((IDawnObject)newObject).DawnInfo = tileSetInfo;
    }
}
