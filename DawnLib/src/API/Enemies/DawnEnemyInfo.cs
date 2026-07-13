using System.Collections.Generic;

namespace Dawn;

public sealed class DawnEnemyInfo : DawnBaseInfo<DawnEnemyInfo>
{
    internal DawnEnemyInfo(NamespacedKey<DawnEnemyInfo> key, HashSet<NamespacedKey> tags, EnemyType enemyType, DawnEnemyLocationInfo? outside, DawnEnemyLocationInfo? inside, DawnEnemyLocationInfo? daytime, DawnEnemyLocationInfo? weed, TerminalNode? bestiaryNode, TerminalKeyword? nameKeyword, IDataContainer? customData) : base(key, tags, customData)
    {
        EnemyType = enemyType;

        Outside = outside;
        if (Outside != null)
        {
            Outside.ParentInfo = this;
        }

        Inside = inside;
        if (Inside != null)
        {
            Inside.ParentInfo = this;
        }

        Daytime = daytime;
        if (Daytime != null)
        {
            Daytime.ParentInfo = this;
        }

        Weed = weed;
        if (Weed != null)
        {
            Weed.ParentInfo = this;
        }

        BestiaryNode = bestiaryNode;
        NameKeyword = nameKeyword;
    }

    public EnemyType EnemyType { get; }

    public DawnEnemyLocationInfo? Outside { get; private set; }
    public DawnEnemyLocationInfo? Inside { get; private set; }
    public DawnEnemyLocationInfo? Daytime { get; private set; }
    public DawnEnemyLocationInfo? Weed { get; private set; }

    public TerminalNode? BestiaryNode { get; internal set; }
    public TerminalKeyword? NameKeyword { get; internal set; }

    public IEnumerable<T> GetAllSpawned<T>(bool strongerCheck = false) where T : EnemyAI
    {
        foreach (EnemyAI enemy in RoundManager.Instance.SpawnedEnemies)
        {
            if (enemy.enemyType == EnemyType || (strongerCheck && enemy.enemyType.enemyName.Equals(EnemyType.enemyName, System.StringComparison.OrdinalIgnoreCase)))
            {
                yield return (T)enemy;
            }
        }
    }
}