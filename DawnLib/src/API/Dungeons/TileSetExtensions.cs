using System;
using System.Diagnostics.CodeAnalysis;
using Dawn.Internal;
using Dawn.Preloader.Interfaces;
using DunGen;

namespace Dawn;

public static class TileSetExtensions
{
    public static NamespacedKey<DawnTileSetInfo> ToNamespacedKey(this TileSet tileSet)
    {
        if (!tileSet.TryGetCRInfo(out DawnTileSetInfo? tileSetInfo))
        {
            Debuggers.Moons?.Log($"TileSet {tileSet} has no CRInfo");
            throw new Exception();
        }
        return tileSetInfo.TypedKey;
    }

    internal static bool TryGetCRInfo(this TileSet tileSet, [NotNullWhen(true)] out DawnTileSetInfo? tileSetInfo)
    {
        object newObject = tileSet;
        tileSetInfo = (DawnTileSetInfo)((ICRObject)newObject).CRInfo;
        return tileSetInfo != null;
    }

    internal static void SetCRInfo(this TileSet tileSet, DawnTileSetInfo tileSetInfo)
    {
        object newObject = tileSet;
        ((ICRObject)newObject).CRInfo = tileSetInfo;
    }
}
