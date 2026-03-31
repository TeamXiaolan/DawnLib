using Dawn.Interfaces;

namespace Dawn;

public static class SpawnableOutsideObjectExtensions
{
    public static DawnMapObjectInfo GetDawnInfo(this SpawnableOutsideObject SpawnableOutsideObject)
    {
        object newObject = SpawnableOutsideObject;
        DawnMapObjectInfo SpawnableOutsideObjectInfo = (DawnMapObjectInfo)((IDawnObject)newObject).DawnInfo;
        return SpawnableOutsideObjectInfo;
    }

    internal static bool HasDawnInfo(this SpawnableOutsideObject SpawnableOutsideObject)
    {
        return SpawnableOutsideObject.GetDawnInfo() != null;
    }

    internal static void SetDawnInfo(this SpawnableOutsideObject SpawnableOutsideObject, DawnMapObjectInfo SpawnableOutsideObjectInfo)
    {
        object newObject = SpawnableOutsideObject;
        ((IDawnObject)newObject).DawnInfo = SpawnableOutsideObjectInfo;
    }
}
