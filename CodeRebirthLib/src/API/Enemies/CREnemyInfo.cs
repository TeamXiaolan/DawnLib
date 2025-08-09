namespace CodeRebirthLib;

public sealed class CREnemyInfo : INamespaced<CREnemyInfo>
{
    internal CREnemyInfo(NamespacedKey<CREnemyInfo> key, EnemyType enemy)
    {
        Enemy = enemy;
        TypedKey = key;
    }
    
    public EnemyType Enemy { get; }
    public NamespacedKey Key => TypedKey;
    public NamespacedKey<CREnemyInfo> TypedKey { get; }
}