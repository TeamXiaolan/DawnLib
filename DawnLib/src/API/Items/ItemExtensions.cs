using System;
using System.Diagnostics.CodeAnalysis;
using Dawn.Internal;
using Dawn.Preloader.Interfaces;

namespace Dawn;

public static class ItemExtensions
{
    public static NamespacedKey<DawnItemInfo> ToNamespacedKey(this Item item)
    {
        if (!item.TryGetCRInfo(out DawnItemInfo? itemInfo))
        {
            Debuggers.Items?.Log($"Item {item} has no CRInfo");
            throw new Exception();
        }
        return itemInfo.TypedKey;
    }

    internal static bool TryGetCRInfo(this Item item, [NotNullWhen(true)] out DawnItemInfo? itemInfo)
    {
        itemInfo = (DawnItemInfo)((ICRObject)item).CRInfo;
        return itemInfo != null;
    }

    internal static void SetCRInfo(this Item item, DawnItemInfo itemInfo)
    {
        ((ICRObject)item).CRInfo = itemInfo;
    }
}
