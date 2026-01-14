using Dawn.Interfaces;

namespace Dusk;

public static class EnemyAIExtensions
{
    public static DuskEnemyReplacementDefinition? GetEnemyReplacement(this EnemyAI enemyAI)
    {
        DuskEnemyReplacementDefinition? enemyReplacementDefinition = (DuskEnemyReplacementDefinition?)((ICurrentEntityReplacement)enemyAI).CurrentEntityReplacement;
        return enemyReplacementDefinition;
    }

    internal static bool HasEnemyReplacement(this EnemyAI enemyAI)
    {
        return enemyAI.GetEnemyReplacement() != null;
    }

    internal static void SetEnemyReplacement(this EnemyAI enemyAI, DuskEnemyReplacementDefinition enemyReplacementDefinition)
    {
        ((ICurrentEntityReplacement)enemyAI).CurrentEntityReplacement = enemyReplacementDefinition;
    }
}
