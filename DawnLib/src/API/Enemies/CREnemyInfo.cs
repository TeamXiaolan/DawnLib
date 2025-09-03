using System.Collections.Generic;

namespace CodeRebirthLib;

public sealed class CREnemyInfo : CRBaseInfo<CREnemyInfo>
{
    internal CREnemyInfo(NamespacedKey<CREnemyInfo> key, List<NamespacedKey> tags, EnemyType enemyType, CREnemyLocationInfo? outside, CREnemyLocationInfo? inside, CREnemyLocationInfo? daytime, TerminalNode? bestiaryNode, TerminalKeyword? nameKeyword) : base(key, tags)
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

    public CREnemyLocationInfo? Outside { get; }
    public CREnemyLocationInfo? Inside { get; }
    public CREnemyLocationInfo? Daytime { get; }
    
    public TerminalNode? BestiaryNode { get; }
    public TerminalKeyword? NameKeyword { get; }
}