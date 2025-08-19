using System.Collections.Generic;

namespace CodeRebirthLib;
public sealed class CRItemInfo : CRBaseInfo<CRItemInfo>
{
    internal CRItemInfo(NamespacedKey<CRItemInfo> key, List<NamespacedKey> tags, Item item, CRScrapItemInfo? scrapItemInfo, CRShopItemInfo? shopItemInfo) : base(key, tags)
    {
        Item = item;
        ScrapInfo = scrapItemInfo;
        if (ScrapInfo != null) ScrapInfo.ParentInfo = this;
        ShopInfo = shopItemInfo;
        if (ShopInfo != null) ShopInfo.ParentInfo = this;
    }

    public Item Item { get; }
    public CRScrapItemInfo? ScrapInfo { get; }
    public CRShopItemInfo? ShopInfo { get; }
}