using System;
using Dawn.Interfaces;

namespace Dawn;

public static class IndoorMapHazardTypeExtensions
{
    extension(IndoorMapHazardType indoorMapHazardType)
    {
        public DawnMapObjectInfo DawnInfo
        {
            get => indoorMapHazardType.GetDawnInfoCore();
            set => indoorMapHazardType.SetDawnInfoCore(value);
        }

        [Obsolete("Use IndoorMapHazardType.DawnInfo instead")]
        public DawnMapObjectInfo GetDawnInfo()
        {
            return indoorMapHazardType.GetDawnInfoCore();
        }

        private DawnMapObjectInfo GetDawnInfoCore()
        {
            return ((IIndoorMapHazardTypeDawnObject)indoorMapHazardType).DawnInfo;
        }

        private void SetDawnInfoCore(DawnMapObjectInfo indoorMapHazardTypeInfo)
        {
            ((IIndoorMapHazardTypeDawnObject)indoorMapHazardType).DawnInfo = indoorMapHazardTypeInfo;
        }
    }
}