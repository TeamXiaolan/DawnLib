using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using CodeRebirthLib.Internal;
using Dawn.Preloader.Interfaces;

namespace CodeRebirthLib;

public static class UnlockableItemExtensions
{
    public static NamespacedKey<CRUnlockableItemInfo> ToNamespacedKey(this UnlockableItem unlockableItem)
    {
        if (!unlockableItem.TryGetCRInfo(out CRUnlockableItemInfo? unlockableItemInfo))
        {
            Debuggers.Unlockables?.Log($"UnlockableItem {unlockableItem} has no CRInfo");
            throw new Exception();
        }
        return unlockableItemInfo.TypedKey;
    }

    internal static bool TryGetCRInfo(this UnlockableItem unlockableItem, [NotNullWhen(true)] out CRUnlockableItemInfo? unlockableItemInfo)
    {
        unlockableItemInfo = (CRUnlockableItemInfo)((ICRObject)unlockableItem).CRInfo;
        return unlockableItemInfo != null;
    }

    internal static void SetCRInfo(this UnlockableItem unlockableItem, CRUnlockableItemInfo unlockableItemInfo)
    {
        ((ICRObject)unlockableItem).CRInfo = unlockableItemInfo;
    }
}
