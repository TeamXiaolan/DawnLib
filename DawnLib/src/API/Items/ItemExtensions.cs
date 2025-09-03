using System;
using System.Diagnostics.CodeAnalysis;
using Dawn.Internal;
using Dawn.Preloader.Interfaces;

namespace Dawn;

public static class ItemExtensions
{
    public static NamespacedKey<DawnItemInfo> ToNamespacedKey(this Item item)
    {
        if (!item.TryGetDawnInfo(out DawnItemInfo? itemInfo))
        {
            Debuggers.Items?.Log($"Item {item} has no CRInfo");
            throw new Exception();
        }
        return itemInfo.TypedKey;
    }

    internal static bool TryGetDawnInfo(this Item item, [NotNullWhen(true)] out DawnItemInfo? itemInfo)
    {
        itemInfo = (DawnItemInfo)((IDawnObject)item).DawnInfo;
        return itemInfo != null;
    }

    internal static void SetDawnInfo(this Item item, DawnItemInfo itemInfo)
    {
        ((IDawnObject)item).DawnInfo = itemInfo;
    }
}
