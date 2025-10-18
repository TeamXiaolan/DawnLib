using Dawn.Interfaces;

namespace Dawn;

public static class UnlockableItemExtensions
{
    public static DawnUnlockableItemInfo GetDawnInfo(this UnlockableItem unlockableItem)
    {
        DawnUnlockableItemInfo unlockableItemInfo = (DawnUnlockableItemInfo)((IDawnObject)unlockableItem).DawnInfo;
        return unlockableItemInfo;
    }

    internal static bool HasDawnInfo(this UnlockableItem unlockableItem)
    {
        return unlockableItem.GetDawnInfo() != null;
    }

    internal static void SetDawnInfo(this UnlockableItem unlockableItem, DawnUnlockableItemInfo unlockableItemInfo)
    {
        ((IDawnObject)unlockableItem).DawnInfo = unlockableItemInfo;
    }
}
