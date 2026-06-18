using System;
using Dawn.Interfaces;

namespace Dawn;

public static class SpawnableOutsideObjectExtensions
{
    extension(SpawnableOutsideObject spawnableOutsideObject)
    {
        public DawnMapObjectInfo DawnInfo
        {
            get => spawnableOutsideObject.GetDawnInfoCore();
            set => spawnableOutsideObject.SetDawnInfoCore(value);
        }

        [Obsolete("Use SpawnableOutsideObject.DawnInfo instead")]
        public DawnMapObjectInfo GetDawnInfo()
        {
            return spawnableOutsideObject.GetDawnInfoCore();
        }

        [Obsolete("Use SpawnableOutsideObject.DawnInfo instead")]
        public void SetDawnInfo(DawnMapObjectInfo mapObjectInfo)
        {
            spawnableOutsideObject.SetDawnInfoCore(mapObjectInfo);
        }

        [Obsolete]
        public bool HasDawnInfo()
        {
            return spawnableOutsideObject.DawnInfo != null;
        }

        private DawnMapObjectInfo GetDawnInfoCore()
        {
            return ((ISpawnableOutsideObjectDawnObject)spawnableOutsideObject).DawnInfo;
        }

        private void SetDawnInfoCore(DawnMapObjectInfo spawnableOutsideObjectInfo)
        {
            ((ISpawnableOutsideObjectDawnObject)spawnableOutsideObject).DawnInfo = spawnableOutsideObjectInfo;
        }
    }
}