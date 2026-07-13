using System;
using System.Collections.Generic;
using System.Linq;
using Dawn.Internal;

namespace Dawn;

public sealed class EnemyListBaseRaritySource : WeightModifierSource<int>
{
    private readonly EnemyType _enemyType;
    private readonly Func<SelectableLevel, List<SpawnableEnemyWithRarity>> _getList;

    public EnemyListBaseRaritySource(EnemyType enemyType, Func<SelectableLevel, List<SpawnableEnemyWithRarity>> getList)
    {
        _enemyType = enemyType;
        _getList = getList;
    }

    public override void Build(WeightBuildContext context, List<IWeightModifier<int>> modifiers)
    {
        foreach (DawnMoonInfo moon in context.Moons.Values)
        {
            SpawnableEnemyWithRarity? entry = _getList(moon.Level).FirstOrDefault(x => x.enemyType == _enemyType);
            if (entry == null)
                continue;

            Debuggers.Enemies?.Log($"Adding inside weight {entry.rarity} for {entry.enemyType} on level {moon.Level.PlanetName}");
            modifiers.Add(new MoonBaseIntModifier(moon.TypedKey, entry.rarity));
        }
    }
}