namespace CodeRebirthLib;
public sealed class CRItemInfo : INamespaced<CRItemInfo>
{
    internal CRItemInfo(NamespacedKey<CRItemInfo> key, Item item, CRScrapItemInfo? scrapItemInfo, CRShopItemInfo? shopItemInfo)
    {
        Item = item;
        ScrapInfo = scrapItemInfo;
        if (ScrapInfo != null) ScrapInfo.ParentInfo = this;
        ShopInfo = shopItemInfo;
        if (ShopInfo != null) ShopInfo.ParentInfo = this;
        TypedKey = key;
    }
    
    public Item Item { get; }
    public CRScrapItemInfo? ScrapInfo { get; }
    public CRShopItemInfo? ShopInfo { get; }
    public NamespacedKey Key => TypedKey;
    public NamespacedKey<CRItemInfo> TypedKey { get; }
}