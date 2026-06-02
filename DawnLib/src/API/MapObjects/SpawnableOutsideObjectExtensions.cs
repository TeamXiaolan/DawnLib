using Dawn.Interfaces;

namespace Dawn;

public static class SpawnableOutsideObjectExtensions
{
    public static DawnMapObjectInfo GetDawnInfo(this SpawnableOutsideObject SpawnableOutsideObject)
    {
        DawnMapObjectInfo SpawnableOutsideObjectInfo = (DawnMapObjectInfo)((IDawnObject)SpawnableOutsideObject).DawnInfo;
        return SpawnableOutsideObjectInfo;
    }

    internal static bool HasDawnInfo(this SpawnableOutsideObject SpawnableOutsideObject)
    {
        return SpawnableOutsideObject.GetDawnInfo() != null;
    }

    internal static void SetDawnInfo(this SpawnableOutsideObject SpawnableOutsideObject, DawnMapObjectInfo SpawnableOutsideObjectInfo)
    {
        ((IDawnObject)SpawnableOutsideObject).DawnInfo = SpawnableOutsideObjectInfo;
    }
}
