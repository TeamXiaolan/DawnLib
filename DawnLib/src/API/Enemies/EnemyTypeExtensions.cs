using System;
using System.Diagnostics.CodeAnalysis;
using Dawn.Internal;
using Dawn.Preloader.Interfaces;

namespace Dawn;

public static class EnemyTypeExtensions
{
    public static NamespacedKey<CREnemyInfo> ToNamespacedKey(this EnemyType enemyType)
    {
        if (!enemyType.TryGetCRInfo(out CREnemyInfo? moonInfo))
        {
            Debuggers.Enemies?.Log($"EnemyType {enemyType} has no CRInfo");
            throw new Exception();
        }
        return moonInfo.TypedKey;
    }

    internal static bool TryGetCRInfo(this EnemyType enemyType, [NotNullWhen(true)] out CREnemyInfo? moonInfo)
    {
        moonInfo = (CREnemyInfo)((ICRObject)enemyType).CRInfo;
        return moonInfo != null;
    }

    internal static void SetCRInfo(this EnemyType enemyType, CREnemyInfo moonInfo)
    {
        ((ICRObject)enemyType).CRInfo = moonInfo;
    }
}
