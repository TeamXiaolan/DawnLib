using System.Collections.Generic;

namespace Dawn;

public sealed class DawnEnemyInfo : DawnBaseInfo<DawnEnemyInfo>
{
    internal DawnEnemyInfo(NamespacedKey<DawnEnemyInfo> key, List<NamespacedKey> tags, EnemyType enemyType, DawnEnemyLocationInfo? outside, DawnEnemyLocationInfo? inside, DawnEnemyLocationInfo? daytime, TerminalNode? bestiaryNode, TerminalKeyword? nameKeyword) : base(key, tags)
    {
        EnemyType = enemyType;
        Outside = outside;
        if (Outside != null) Outside.ParentInfo = this;
        Inside = inside;
        if (Inside != null) Inside.ParentInfo = this;
        Daytime = daytime;
        if (Daytime != null) Daytime.ParentInfo = this;
        BestiaryNode = bestiaryNode;
        NameKeyword = nameKeyword;
    }

    public EnemyType EnemyType { get; }

    public DawnEnemyLocationInfo? Outside { get; }
    public DawnEnemyLocationInfo? Inside { get; }
    public DawnEnemyLocationInfo? Daytime { get; }
    
    public TerminalNode? BestiaryNode { get; }
    public TerminalKeyword? NameKeyword { get; }
}