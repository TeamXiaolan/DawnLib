using Dawn.Interfaces;

namespace Dawn;

public static class IndoorMapHazardTypeExtensions
{
    public static DawnMapObjectInfo GetDawnInfo(this IndoorMapHazardType IndoorMapHazardType)
    {
        object newObject = IndoorMapHazardType;
        DawnMapObjectInfo IndoorMapHazardTypeInfo = (DawnMapObjectInfo)((IDawnObject)newObject).DawnInfo;
        return IndoorMapHazardTypeInfo;
    }

    internal static bool HasDawnInfo(this IndoorMapHazardType IndoorMapHazardType)
    {
        return IndoorMapHazardType.GetDawnInfo() != null;
    }

    internal static void SetDawnInfo(this IndoorMapHazardType IndoorMapHazardType, DawnMapObjectInfo IndoorMapHazardTypeInfo)
    {
        object newObject = IndoorMapHazardType;
        ((IDawnObject)newObject).DawnInfo = IndoorMapHazardTypeInfo;
    }
}
