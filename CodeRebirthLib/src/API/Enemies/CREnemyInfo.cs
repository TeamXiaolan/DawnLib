namespace CodeRebirthLib;

public sealed class CREnemyInfo : INamespaced<CREnemyInfo>
{
    internal CREnemyInfo(NamespacedKey<CREnemyInfo> key, EnemyType enemy, Table<int?,CRMoonInfo>? outsideWeights, Table<int?,CRMoonInfo>? insideWeights, Table<int?,CRMoonInfo>? daytimeWeights)
    {
        Enemy = enemy;
        TypedKey = key;
        OutsideWeights = outsideWeights;
        InsideWeights = insideWeights;
        DaytimeWeights = daytimeWeights;
    }
    
    public EnemyType Enemy { get; }
    
    public Table<int?,CRMoonInfo>? OutsideWeights { get; }
    public Table<int?,CRMoonInfo>? InsideWeights { get; }
    public Table<int?,CRMoonInfo>? DaytimeWeights { get; }
    
    public NamespacedKey Key => TypedKey;
    public NamespacedKey<CREnemyInfo> TypedKey { get; }
}