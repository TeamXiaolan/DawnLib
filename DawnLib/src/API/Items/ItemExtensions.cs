using Dawn.Interfaces;

namespace Dawn;

public static class ItemExtensions
{
    public static DawnItemInfo GetDawnInfo(this Item item)
    {
        DawnItemInfo itemInfo = (DawnItemInfo)((IDawnObject)item).DawnInfo;
        return itemInfo;
    }

    internal static bool HasDawnInfo(this Item item)
    {
        return item.GetDawnInfo() != null;
    }

    internal static void SetDawnInfo(this Item item, DawnItemInfo itemInfo)
    {
        ((IDawnObject)item).DawnInfo = itemInfo;
    }
}
