using System.Collections.Generic;
using Dawn;

namespace Dusk.Weights;

public sealed class DynamicItemSkinSource : WeightModifierSource<int>
{
    private readonly NamespacedKey _keyOfRelevantSkin;

    public DynamicItemSkinSource(NamespacedKey keyOfRelevantSkin)
    {
        _keyOfRelevantSkin = keyOfRelevantSkin;
    }

    public override void Build(WeightBuildContext context, List<IWeightModifier<int>> modifiers)
    {
        modifiers.Add(new DynamicItemSkinModifier(_keyOfRelevantSkin));
    }
}