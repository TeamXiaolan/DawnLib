namespace Dawn;
public sealed class DawnEnemyLocationInfo
{
    public DawnEnemyInfo ParentInfo { get; internal set; }

    internal DawnEnemyLocationInfo(ProviderTable<int?, DawnMoonInfo> weights)
    {
        Weights = weights;
    }

    public ProviderTable<int?, DawnMoonInfo> Weights { get; private set; }
}