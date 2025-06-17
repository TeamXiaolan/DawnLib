using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace CodeRebirthLib.ContentManagement.Enemies;
public static class CRModEnemyExtensions
{
    public static bool TryGetFromEnemyName(this CRRegistry<CREnemyDefinition> registry, string enemyName, [NotNullWhen(true)] out CREnemyDefinition? value)
    {
        return registry.TryGetFirstBySomeName(it => it.EnemyType.enemyName, enemyName, out value);
    }

    public static bool TryGetDefinition(this EnemyType type, [NotNullWhen(true)] out CREnemyDefinition? definition)
    {
        definition = CRMod.AllEnemies().FirstOrDefault(it => it.EnemyType.enemyName == type.enemyName);
        return definition; // implict cast
    }
}