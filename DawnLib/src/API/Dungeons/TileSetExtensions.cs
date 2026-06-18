using System;
using Dawn.Interfaces;
using DunGen;

namespace Dawn;

public static class TileSetExtensions
{
    extension(TileSet tileSet)
    {
        public DawnTileSetInfo DawnInfo
        {
            get => tileSet.GetDawnInfoCore();
            set => tileSet.SetDawnInfoCore(value);
        }

        [Obsolete("Use TileSet.DawnInfo instead")]
        public DawnTileSetInfo GetDawnInfo()
        {
            return tileSet.GetDawnInfoCore();
        }

        [Obsolete("Use TileSet.DawnInfo instead")]
        public void SetDawnInfo(DawnTileSetInfo tileSetInfo)
        {
            tileSet.SetDawnInfoCore(tileSetInfo);
        }

        [Obsolete]
        public bool HasDawnInfo()
        {
            return tileSet.DawnInfo != null;
        }

        private DawnTileSetInfo GetDawnInfoCore()
        {
            object newObject = tileSet;
            return ((IDunGenTileSetDawnObject)newObject).DawnInfo;
        }

        private void SetDawnInfoCore(DawnTileSetInfo tileSetInfo)
        {
            object newObject = tileSet;
            ((IDunGenTileSetDawnObject)newObject).DawnInfo = tileSetInfo;
        }
    }
}
