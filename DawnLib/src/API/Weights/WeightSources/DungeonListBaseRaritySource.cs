using System.Collections.Generic;
using System.Linq;
using Dawn.Internal;
using DunGen.Graph;

namespace Dawn;

public sealed class DungeonListBaseRaritySource : WeightModifierSource<int>
{
    private readonly DungeonFlow _targetDungeonFlow;

    public DungeonListBaseRaritySource(DungeonFlow targetDungeonFlow)
    {
        _targetDungeonFlow = targetDungeonFlow;
    }

    public override void Build(WeightBuildContext context, List<IWeightModifier<int>> modifiers)
    {
        foreach (DawnMoonInfo moonInfo in context.Moons.Values)
        {
            SelectableLevel level = moonInfo.Level;

            IntWithRarity? rarityEntry = FindDungeonRarity(level, _targetDungeonFlow);

            if (rarityEntry == null)
                continue;

            Debuggers.Dungeons?.Log($"Adding weight {rarityEntry.rarity} for {_targetDungeonFlow} on level {level.PlanetName}");
            modifiers.Add(new MoonBaseIntModifier(moonInfo.TypedKey, rarityEntry.rarity));
        }
    }

    private static IntWithRarity? FindDungeonRarity(SelectableLevel level, DungeonFlow targetDungeonFlow)
    {
        IEnumerable<IntWithRarity> entries = level.dungeonFlowTypes;
        if (LethalLevelLoaderCompat.Enabled)
        {
            entries = entries.Concat(LethalLevelLoaderCompat.GetCustomDungeonsWithRarities(level));
        }

        foreach (IntWithRarity entry in entries)
        {
            if (!TryGetDungeonFlow(entry, out DungeonFlow? entryDungeonFlow))
                continue;

            if (entryDungeonFlow == targetDungeonFlow)
                return entry;
        }

        return null;
    }

    private static bool TryGetDungeonFlow(IntWithRarity entry, out DungeonFlow? dungeonFlow)
    {
        dungeonFlow = null;
        if (entry.id < 0)
            return false;

        IndoorMapType[] dungeonTypes = RoundManagerRefs.Instance.dungeonFlowTypes;

        if (entry.id >= dungeonTypes.Length)
            return false;

        dungeonFlow = dungeonTypes[entry.id]?.dungeonFlow;
        return dungeonFlow != null;
    }
}