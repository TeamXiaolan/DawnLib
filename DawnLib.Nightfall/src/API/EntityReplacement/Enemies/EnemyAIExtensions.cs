using System;
using System.Diagnostics.CodeAnalysis;
using Nightfall.Interfaces;

namespace Nightfall;

public static class EnemyAIExtensions
{
    public static bool TryGetEnemyReplacement(this EnemyAI enemyAI, [NotNullWhen(true)] out NightfallEnemyReplacementDefinition? replacement)
    {
        replacement = ((ICurrentEntityReplacement)enemyAI).CurrentEntityReplacement as NightfallEnemyReplacementDefinition;
        return replacement != null;
    }

    [Obsolete($"Use {nameof(TryGetEnemyReplacement)}")]
    public static NightfallEnemyReplacementDefinition? GetEnemyReplacement(this EnemyAI enemyAI)
    {
        enemyAI.TryGetEnemyReplacement(out var replacement);
        return replacement;
    }

    internal static void SetEnemyReplacement(this EnemyAI enemyAI, NightfallEnemyReplacementDefinition enemyReplacementDefinition)
    {
        ((ICurrentEntityReplacement)enemyAI).CurrentEntityReplacement = enemyReplacementDefinition;
    }
}
