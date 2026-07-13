using System.Collections.Generic;
using System.Linq;
using Dawn.Internal;

namespace Dawn;

public sealed class ItemListBaseRaritySource : WeightModifierSource<int>
{
    private readonly Item _item;

    public ItemListBaseRaritySource(Item item)
    {
        _item = item;
    }

    public override void Build(WeightBuildContext context, List<IWeightModifier<int>> modifiers)
    {
        foreach (DawnMoonInfo moon in context.Moons.Values)
        {
            SpawnableItemWithRarity? entry = moon.Level.spawnableScrap.FirstOrDefault(x => x.spawnableItem == _item);
            if (entry == null)
                continue;

            Debuggers.Items?.Log($"Adding weight {entry.rarity} for {entry.spawnableItem} on level {moon.Level.PlanetName}");
            modifiers.Add(new MoonBaseIntModifier(moon.TypedKey, entry.rarity));
        }
    }
}