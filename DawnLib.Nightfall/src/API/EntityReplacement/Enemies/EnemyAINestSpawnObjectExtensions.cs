using Nightfall.Interfaces;

namespace Nightfall;

public static class EnemyAINestSpawnObjectExtensions
{
    public static NightfallEnemyReplacementDefinition? GetNestReplacement(this EnemyAINestSpawnObject nest)
    {
        NightfallEnemyReplacementDefinition? enemyReplacementDefinition = (NightfallEnemyReplacementDefinition?)((ICurrentEntityReplacement)nest).CurrentEntityReplacement;
        return enemyReplacementDefinition;
    }

    internal static bool HasNestReplacement(this EnemyAINestSpawnObject nest)
    {
        return nest.GetNestReplacement() != null;
    }

    internal static void SetNestReplacement(this EnemyAINestSpawnObject nestSpawnObject, NightfallEnemyReplacementDefinition enemyReplacementDefinition)
    {
        ((ICurrentEntityReplacement)nestSpawnObject).CurrentEntityReplacement = enemyReplacementDefinition;
    }
}