using Dawn.Interfaces;

namespace Dawn;

public static class IndoorMapHazardTypeExtensions
{
    public static DawnMapObjectInfo GetDawnInfo(this IndoorMapHazardType IndoorMapHazardType)
    {
        DawnMapObjectInfo IndoorMapHazardTypeInfo = (DawnMapObjectInfo)((IDawnObject)IndoorMapHazardType).DawnInfo;
        return IndoorMapHazardTypeInfo;
    }

    internal static bool HasDawnInfo(this IndoorMapHazardType IndoorMapHazardType)
    {
        return IndoorMapHazardType.GetDawnInfo() != null;
    }

    internal static void SetDawnInfo(this IndoorMapHazardType IndoorMapHazardType, DawnMapObjectInfo IndoorMapHazardTypeInfo)
    {
        ((IDawnObject)IndoorMapHazardType).DawnInfo = IndoorMapHazardTypeInfo;
    }
}
