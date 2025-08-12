using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CodeRebirthLib.CRMod;

namespace CodeRebirthLib;
public static class CREnemyExtensions
{
    public static bool TryGetDefinition(this EnemyType type, [NotNullWhen(true)] out CREnemyDefinition? definition)
    {
        definition = LethalContent.Enemies.Values.FirstOrDefault(it => it.Enemy == type);
        if (!definition) Debuggers.ReplaceThis?.Log($"TryGetDefinition for EnemyDefinition failed with {type.enemyName}");
        return definition; // implict cast
    }
}