using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Dawn.Internal;
using Dawn.Preloader.Interfaces;

namespace Dawn;

public static class UnlockableItemExtensions
{
    public static NamespacedKey<DawnUnlockableItemInfo> ToNamespacedKey(this UnlockableItem unlockableItem)
    {
        if (!unlockableItem.TryGetCRInfo(out DawnUnlockableItemInfo? unlockableItemInfo))
        {
            Debuggers.Unlockables?.Log($"UnlockableItem {unlockableItem} has no CRInfo");
            throw new Exception();
        }
        return unlockableItemInfo.TypedKey;
    }

    internal static bool TryGetCRInfo(this UnlockableItem unlockableItem, [NotNullWhen(true)] out DawnUnlockableItemInfo? unlockableItemInfo)
    {
        unlockableItemInfo = (DawnUnlockableItemInfo)((ICRObject)unlockableItem).CRInfo;
        return unlockableItemInfo != null;
    }

    internal static void SetCRInfo(this UnlockableItem unlockableItem, DawnUnlockableItemInfo unlockableItemInfo)
    {
        ((ICRObject)unlockableItem).CRInfo = unlockableItemInfo;
    }
}
