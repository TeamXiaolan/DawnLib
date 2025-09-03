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
        if (!tileSet.TryGetDawnInfo(out DawnTileSetInfo? tileSetInfo))
        {
            Debuggers.Moons?.Log($"TileSet {tileSet} has no CRInfo");
            throw new Exception();
        }
        return tileSetInfo.TypedKey;
    }

    internal static bool TryGetDawnInfo(this TileSet tileSet, [NotNullWhen(true)] out DawnTileSetInfo? tileSetInfo)
    {
        object newObject = tileSet;
        tileSetInfo = (DawnTileSetInfo)((IDawnObject)newObject).DawnInfo;
        return tileSetInfo != null;
    }

    internal static void SetDawnInfo(this TileSet tileSet, DawnTileSetInfo tileSetInfo)
    {
        object newObject = tileSet;
        ((IDawnObject)newObject).DawnInfo = tileSetInfo;
    }
}
