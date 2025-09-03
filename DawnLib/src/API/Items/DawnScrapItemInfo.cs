namespace Dawn;
public sealed class DawnScrapItemInfo
{
    public DawnItemInfo ParentInfo { get; internal set; }

    internal DawnScrapItemInfo(ProviderTable<int?, DawnMoonInfo> weights)
    {
        Weights = weights;
    }

    public ProviderTable<int?, DawnMoonInfo> Weights { get; }
}