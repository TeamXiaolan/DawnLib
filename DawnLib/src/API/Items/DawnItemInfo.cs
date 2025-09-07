using System.Collections.Generic;

namespace Dawn;
public sealed class DawnItemInfo : DawnBaseInfo<DawnItemInfo>
{
    internal DawnItemInfo(NamespacedKey<DawnItemInfo> key, List<NamespacedKey> tags, Item item, DawnScrapItemInfo? scrapItemInfo, DawnShopItemInfo? shopItemInfo, DataContainer? customData) : base(key, tags, customData)
    {
        Item = item;
        ScrapInfo = scrapItemInfo;
        if (ScrapInfo != null) ScrapInfo.ParentInfo = this;
        ShopInfo = shopItemInfo;
        if (ShopInfo != null) ShopInfo.ParentInfo = this;
    }

    public Item Item { get; }
    public DawnScrapItemInfo? ScrapInfo { get; }
    public DawnShopItemInfo? ShopInfo { get; }
}