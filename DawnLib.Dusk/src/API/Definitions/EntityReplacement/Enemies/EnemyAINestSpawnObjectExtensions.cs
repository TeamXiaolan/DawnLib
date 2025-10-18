using Dawn.Interfaces;

namespace Dusk;

public static class EnemyAINestSpawnObjectExtensions
{
    public static DuskEnemyReplacementDefinition? GetNestReplacement(this EnemyAINestSpawnObject nest)
    {
        DuskEnemyReplacementDefinition? enemyReplacementDefinition = (DuskEnemyReplacementDefinition?)((ICurrentEntityReplacement)nest).CurrentEntityReplacement;
        return enemyReplacementDefinition;
    }

    internal static bool HasNestReplacement(this EnemyAINestSpawnObject nest)
    {
        return nest.GetNestReplacement() != null;
    }

    internal static void SetNestReplacement(this EnemyAINestSpawnObject nestSpawnObject, DuskEnemyReplacementDefinition enemyReplacementDefinition)
    {
        ((ICurrentEntityReplacement)nestSpawnObject).CurrentEntityReplacement = enemyReplacementDefinition;
    }
}