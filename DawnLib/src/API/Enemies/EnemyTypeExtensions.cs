using System;
using System.Diagnostics.CodeAnalysis;
using Dawn.Internal;
using Dawn.Preloader.Interfaces;

namespace Dawn;

public static class EnemyTypeExtensions
{
    public static NamespacedKey<DawnEnemyInfo> ToNamespacedKey(this EnemyType enemyType)
    {
        if (!enemyType.TryGetDawnInfo(out DawnEnemyInfo? moonInfo))
        {
            Debuggers.Enemies?.Log($"EnemyType {enemyType} has no CRInfo");
            throw new Exception();
        }
        return moonInfo.TypedKey;
    }

    internal static bool TryGetDawnInfo(this EnemyType enemyType, [NotNullWhen(true)] out DawnEnemyInfo? moonInfo)
    {
        moonInfo = (DawnEnemyInfo)((IDawnObject)enemyType).DawnInfo;
        return moonInfo != null;
    }

    internal static void SetDawnInfo(this EnemyType enemyType, DawnEnemyInfo moonInfo)
    {
        ((IDawnObject)enemyType).DawnInfo = moonInfo;
    }
}
