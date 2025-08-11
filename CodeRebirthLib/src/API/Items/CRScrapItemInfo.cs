namespace CodeRebirthLib;
public sealed class CRScrapItemInfo
{
    public CRItemInfo ParentInfo { get; internal set; }

    internal CRScrapItemInfo(ProviderTable<int?, CRMoonInfo> weights)
    {
        Weights = weights;
    }
    
    public ProviderTable<int?, CRMoonInfo> Weights { get; }
}