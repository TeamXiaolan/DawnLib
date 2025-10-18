using Dawn.Interfaces;

namespace Dawn;

public static class EnemyTypeExtensions
{
    public static DawnEnemyInfo GetDawnInfo(this EnemyType enemyType)
    {
        DawnEnemyInfo enemyInfo = (DawnEnemyInfo)((IDawnObject)enemyType).DawnInfo;
        return enemyInfo;
    }

    internal static bool HasDawnInfo(this EnemyType enemyType)
    {
        return enemyType.GetDawnInfo() != null;
    }

    internal static void SetDawnInfo(this EnemyType enemyType, DawnEnemyInfo enemyInfo)
    {
        ((IDawnObject)enemyType).DawnInfo = enemyInfo;
    }
}
