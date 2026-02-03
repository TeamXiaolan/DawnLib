using System;
using System.Diagnostics.CodeAnalysis;
using Dawn.Interfaces;

namespace Dusk;

public static class EnemyAIExtensions
{
    public static bool TryGetEnemyReplacement(this EnemyAI enemyAI, [NotNullWhen(true)] out DuskEnemyReplacementDefinition? replacement)
    {
        replacement = ((ICurrentEntityReplacement)enemyAI).CurrentEntityReplacement as DuskEnemyReplacementDefinition;
        return replacement != null;
    }

    [Obsolete($"Use {nameof(TryGetEnemyReplacement)}")]
    public static DuskEnemyReplacementDefinition? GetEnemyReplacement(this EnemyAI enemyAI)
    {
        enemyAI.TryGetEnemyReplacement(out var replacement);
        return replacement;
    }

    internal static void SetEnemyReplacement(this EnemyAI enemyAI, DuskEnemyReplacementDefinition enemyReplacementDefinition)
    {
        ((ICurrentEntityReplacement)enemyAI).CurrentEntityReplacement = enemyReplacementDefinition;
    }
}
