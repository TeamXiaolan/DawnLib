using System;
using System.Diagnostics.CodeAnalysis;
using CodeRebirthLib.Internal;
using CodeRebirthLib.Preloader;
using DunGen;

namespace CodeRebirthLib;

public static class DungeonArchetypeExtensions
{
    public static NamespacedKey<CRArchetypeInfo> ToNamespacedKey(this DungeonArchetype archetype)
    {
        if (!archetype.TryGetCRInfo(out CRArchetypeInfo? tileSetInfo))
        {
            Debuggers.Dungeons?.Log($"Archetype {archetype} has no CRInfo");
            throw new Exception();
        }
        return tileSetInfo.TypedKey;
    }

    internal static bool TryGetCRInfo(this DungeonArchetype archetype, [NotNullWhen(true)] out CRArchetypeInfo? tileSetInfo)
    {
        object newObject = archetype;
        tileSetInfo = (CRArchetypeInfo)((ICRObject)newObject).CRInfo;
        return tileSetInfo != null;
    }

    internal static void SetCRInfo(this DungeonArchetype archetype, CRArchetypeInfo tileSetInfo)
    {
        object newObject = archetype;
        ((ICRObject)newObject).CRInfo = tileSetInfo;
    }
}
