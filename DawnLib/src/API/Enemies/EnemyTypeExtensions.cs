using System;
using System.Diagnostics.CodeAnalysis;
using Dawn.Internal;
using Dawn.Preloader.Interfaces;

namespace Dawn;

public static class EnemyTypeExtensions
{
    public static NamespacedKey<DawnEnemyInfo> ToNamespacedKey(this EnemyType enemyType)
    {
        if (!enemyType.TryGetCRInfo(out DawnEnemyInfo? moonInfo))
        {
            Debuggers.Enemies?.Log($"EnemyType {enemyType} has no CRInfo");
            throw new Exception();
        }
        return moonInfo.TypedKey;
    }

    internal static bool TryGetCRInfo(this EnemyType enemyType, [NotNullWhen(true)] out DawnEnemyInfo? moonInfo)
    {
        moonInfo = (DawnEnemyInfo)((ICRObject)enemyType).CRInfo;
        return moonInfo != null;
    }

    internal static void SetCRInfo(this EnemyType enemyType, DawnEnemyInfo moonInfo)
    {
        ((ICRObject)enemyType).CRInfo = moonInfo;
    }
}
