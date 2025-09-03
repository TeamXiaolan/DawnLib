using System;
using System.Diagnostics.CodeAnalysis;
using CodeRebirthLib.Internal;
using Dawn.Preloader.Interfaces;
using DunGen;

namespace CodeRebirthLib;

public static class TileSetExtensions
{
    public static NamespacedKey<CRTileSetInfo> ToNamespacedKey(this TileSet tileSet)
    {
        if (!tileSet.TryGetCRInfo(out CRTileSetInfo? tileSetInfo))
        {
            Debuggers.Moons?.Log($"TileSet {tileSet} has no CRInfo");
            throw new Exception();
        }
        return tileSetInfo.TypedKey;
    }

    internal static bool TryGetCRInfo(this TileSet tileSet, [NotNullWhen(true)] out CRTileSetInfo? tileSetInfo)
    {
        object newObject = tileSet;
        tileSetInfo = (CRTileSetInfo)((ICRObject)newObject).CRInfo;
        return tileSetInfo != null;
    }

    internal static void SetCRInfo(this TileSet tileSet, CRTileSetInfo tileSetInfo)
    {
        object newObject = tileSet;
        ((ICRObject)newObject).CRInfo = tileSetInfo;
    }
}
