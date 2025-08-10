namespace CodeRebirthLib;

public sealed class CREnemyInfo : INamespaced<CREnemyInfo>
{
    internal CREnemyInfo(NamespacedKey<CREnemyInfo> key, EnemyType enemy, WeightTable<CRMoonInfo>? outsideWeights, WeightTable<CRMoonInfo>? insideWeights, WeightTable<CRMoonInfo>? daytimeWeights)
    {
        Enemy = enemy;
        TypedKey = key;
        OutsideWeights = outsideWeights;
        InsideWeights = insideWeights;
        DaytimeWeights = daytimeWeights;
    }
    
    public EnemyType Enemy { get; }
    
    public WeightTable<CRMoonInfo>? OutsideWeights { get; }
    public WeightTable<CRMoonInfo>? InsideWeights { get; }
    public WeightTable<CRMoonInfo>? DaytimeWeights { get; }
    
    public NamespacedKey Key => TypedKey;
    public NamespacedKey<CREnemyInfo> TypedKey { get; }
}