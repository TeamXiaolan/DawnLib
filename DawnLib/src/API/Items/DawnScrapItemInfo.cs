namespace Dawn;
public sealed class DawnScrapItemInfo
{
    public DawnItemInfo ParentInfo { get; internal set; }

    internal DawnScrapItemInfo(ProviderTable<int?, DawnMoonInfo, SpawnWeightContext> weights)
    {
        Weights = weights;
    }

    public ProviderTable<int?, DawnMoonInfo, SpawnWeightContext> Weights { get; private set; }
}