namespace CodeRebirthLib;

public class EnemyInfoBuilder
{
    private NamespacedKey<CREnemyInfo> _key;
    private EnemyType _enemy;
    
    internal EnemyInfoBuilder(NamespacedKey<CREnemyInfo> key, EnemyType enemy)
    {
        _key = key;
        _enemy = enemy;
    }

    internal CREnemyInfo Build()
    {
        return new CREnemyInfo(_key, _enemy);
    }
}