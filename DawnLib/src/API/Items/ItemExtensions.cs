using System;
using System.Diagnostics.CodeAnalysis;
using CodeRebirthLib.Internal;
using Dawn.Preloader.Interfaces;

namespace CodeRebirthLib;

public static class ItemExtensions
{
    public static NamespacedKey<CRItemInfo> ToNamespacedKey(this Item item)
    {
        if (!item.TryGetCRInfo(out CRItemInfo? itemInfo))
        {
            Debuggers.Items?.Log($"Item {item} has no CRInfo");
            throw new Exception();
        }
        return itemInfo.TypedKey;
    }

    internal static bool TryGetCRInfo(this Item item, [NotNullWhen(true)] out CRItemInfo? itemInfo)
    {
        itemInfo = (CRItemInfo)((ICRObject)item).CRInfo;
        return itemInfo != null;
    }

    internal static void SetCRInfo(this Item item, CRItemInfo itemInfo)
    {
        ((ICRObject)item).CRInfo = itemInfo;
    }
}
