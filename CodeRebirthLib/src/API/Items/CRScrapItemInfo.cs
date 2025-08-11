namespace CodeRebirthLib;
public sealed class CRScrapItemInfo
{
    public CRItemInfo ParentInfo { get; internal set; }

    internal CRScrapItemInfo(Table<int?, CRMoonInfo> weights)
    {
        Weights = weights;
    }
    
    public Table<int?, CRMoonInfo> Weights { get; private set; }
}