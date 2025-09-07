using System;
using System.Diagnostics.CodeAnalysis;
using Dawn.Internal;
using Dawn.Preloader.Interfaces;

namespace Dawn;

public static class UnlockableItemExtensions
{
    public static NamespacedKey<DawnUnlockableItemInfo> ToNamespacedKey(this UnlockableItem unlockableItem)
    {
        if (!unlockableItem.TryGetDawnInfo(out DawnUnlockableItemInfo? unlockableItemInfo))
        {
            Debuggers.Unlockables?.Log($"UnlockableItem {unlockableItem} has no CRInfo");
            throw new Exception();
        }
        return unlockableItemInfo.TypedKey;
    }

    internal static bool TryGetDawnInfo(this UnlockableItem unlockableItem, [NotNullWhen(true)] out DawnUnlockableItemInfo? unlockableItemInfo)
    {
        unlockableItemInfo = (DawnUnlockableItemInfo)((IDawnObject)unlockableItem).DawnInfo;
        return unlockableItemInfo != null;
    }

    internal static void SetDawnInfo(this UnlockableItem unlockableItem, DawnUnlockableItemInfo unlockableItemInfo)
    {
        ((IDawnObject)unlockableItem).DawnInfo = unlockableItemInfo;
    }
}
