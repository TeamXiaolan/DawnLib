namespace CodeRebirthLib;
public sealed class CRScrapItemInfo
{
    public CRItemInfo ParentInfo { get; internal set; }

    internal CRScrapItemInfo(WeightTable<CRMoonInfo> weights)
    {
        Weights = weights;
    }
    
    public WeightTable<CRMoonInfo> Weights { get; private set; }
}