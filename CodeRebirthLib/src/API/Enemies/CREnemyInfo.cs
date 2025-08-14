namespace CodeRebirthLib;

public sealed class CREnemyInfo : CRBaseInfo<CREnemyInfo>
{
    internal CREnemyInfo(NamespacedKey<CREnemyInfo> key, bool isExternal, EnemyType enemy, ProviderTable<int?, CRMoonInfo>? outsideWeights, ProviderTable<int?, CRMoonInfo>? insideWeights, ProviderTable<int?, CRMoonInfo>? daytimeWeights) : base(key, isExternal)
    {
        Enemy = enemy;
        OutsideWeights = outsideWeights;
        InsideWeights = insideWeights;
        DaytimeWeights = daytimeWeights;
    }

    public EnemyType Enemy { get; }

    public ProviderTable<int?, CRMoonInfo>? OutsideWeights { get; }
    public ProviderTable<int?, CRMoonInfo>? InsideWeights { get; }
    public ProviderTable<int?, CRMoonInfo>? DaytimeWeights { get; }
}