namespace CodeRebirthLib;
public class CREnemyLocationInfo
{
    public CREnemyInfo ParentInfo { get; internal set; }

    internal CREnemyLocationInfo(ProviderTable<int?, CRMoonInfo>? weights)
    {
        Weights = weights;
    }
    
    public ProviderTable<int?, CRMoonInfo>? Weights { get; }
}