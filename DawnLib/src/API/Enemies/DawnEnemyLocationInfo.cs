namespace Dawn;
public sealed class DawnEnemyLocationInfo
{
    public DawnEnemyInfo ParentInfo { get; internal set; }

    internal DawnEnemyLocationInfo(ProviderTable<int?, DawnMoonInfo, SpawnWeightContext> weights)
    {
        Weights = weights;
    }

    public ProviderTable<int?, DawnMoonInfo, SpawnWeightContext> Weights { get; private set; }
}