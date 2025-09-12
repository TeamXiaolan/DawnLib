using System.Collections.Generic;
using System.Reflection;

namespace Dawn;

public sealed class DawnEnemyInfo : DawnBaseInfo<DawnEnemyInfo>
{
    internal DawnEnemyInfo(NamespacedKey<DawnEnemyInfo> key, HashSet<NamespacedKey> tags, EnemyType enemyType, DawnEnemyLocationInfo? outside, DawnEnemyLocationInfo? inside, DawnEnemyLocationInfo? daytime, TerminalNode? bestiaryNode, TerminalKeyword? nameKeyword, IDataContainer? customData) : base(key, tags, customData)
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

        EnemyAI enemyAI = enemyType.enemyPrefab.GetComponent<EnemyAI>();
        foreach (FieldInfo fieldInfo in enemyAI.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic))
        {
            FieldNameFieldInfoDict.Add(fieldInfo.Name.ToLowerInvariant(), fieldInfo);
        }
    }

    public EnemyType EnemyType { get; }

    public DawnEnemyLocationInfo? Outside { get; }
    public DawnEnemyLocationInfo? Inside { get; }
    public DawnEnemyLocationInfo? Daytime { get; }

    public TerminalNode? BestiaryNode { get; }
    public TerminalKeyword? NameKeyword { get; }

    public Dictionary<string, FieldInfo> FieldNameFieldInfoDict { get; } = new();

    public IEnumerable<T> GetAllSpawned<T>() where T : EnemyAI
    {
        foreach (EnemyAI enemy in RoundManager.Instance.SpawnedEnemies)
        {
            if (enemy.enemyType == EnemyType)
                yield return (T)enemy;
        }
    }
}