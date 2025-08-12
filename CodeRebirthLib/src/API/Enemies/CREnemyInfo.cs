namespace CodeRebirthLib;

public sealed class CREnemyInfo : INamespaced<CREnemyInfo>
{
    internal CREnemyInfo(NamespacedKey<CREnemyInfo> key, EnemyType enemy, ProviderTable<int?, CRMoonInfo>? outsideWeights, ProviderTable<int?, CRMoonInfo>? insideWeights, ProviderTable<int?, CRMoonInfo>? daytimeWeights)
    {
        Enemy = enemy;
        TypedKey = key;
        OutsideWeights = outsideWeights;
        InsideWeights = insideWeights;
        DaytimeWeights = daytimeWeights;
    }
    
    public EnemyType Enemy { get; }
    
    public ProviderTable<int?, CRMoonInfo>? OutsideWeights { get; }
    public ProviderTable<int?, CRMoonInfo>? InsideWeights { get; }
    public ProviderTable<int?, CRMoonInfo>? DaytimeWeights { get; }
    
    public NamespacedKey Key => TypedKey;
    public NamespacedKey<CREnemyInfo> TypedKey { get; }
}