using Dawn.Preloader.Interfaces;

namespace Dusk;

public static class EnemyTypeExtensions
{
    public static DuskEnemyReplacementDefinition? GetEnemyReplacement(this EnemyType enemyType)
    {
        DuskEnemyReplacementDefinition? enemyReplacementDefinition = (DuskEnemyReplacementDefinition?)((ICurrentEntityReplacement)enemyType).CurrentEntityReplacement;
        return enemyReplacementDefinition;
    }

    internal static bool HasDawnInfo(this EnemyType enemyType)
    {
        return enemyType.GetEnemyReplacement() != null;
    }

    internal static void SetEnemyReplacement(this EnemyType enemyType, DuskEnemyReplacementDefinition enemyReplacementDefinition)
    {
        ((ICurrentEntityReplacement)enemyType).CurrentEntityReplacement = enemyReplacementDefinition;
    }
}
