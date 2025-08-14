namespace CodeRebirthLib;
public sealed class CRItemInfo : CRBaseInfo<CRItemInfo>
{
    internal CRItemInfo(NamespacedKey<CRItemInfo> key, bool isExternal, Item item, CRScrapItemInfo? scrapItemInfo, CRShopItemInfo? shopItemInfo) : base(key, isExternal)
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