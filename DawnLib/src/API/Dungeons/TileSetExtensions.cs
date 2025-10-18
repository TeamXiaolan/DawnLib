using Dawn.Interfaces;
using DunGen;

namespace Dawn;

public static class TileSetExtensions
{
    public static DawnTileSetInfo GetDawnInfo(this TileSet tileSet)
    {
        object newObject = tileSet;
        DawnTileSetInfo tileSetInfo = (DawnTileSetInfo)((IDawnObject)newObject).DawnInfo;
        return tileSetInfo;
    }

    internal static bool HasDawnInfo(this TileSet tileSet)
    {
        return tileSet.GetDawnInfo() != null;
    }

    internal static void SetDawnInfo(this TileSet tileSet, DawnTileSetInfo tileSetInfo)
    {
        object newObject = tileSet;
        ((IDawnObject)newObject).DawnInfo = tileSetInfo;
    }
}
