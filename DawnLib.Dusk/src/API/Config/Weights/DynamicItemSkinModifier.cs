using System.Collections.Generic;
using Dawn;

namespace Dusk.Weights;

public sealed class DynamicItemSkinModifier : IWeightModifier<int>
{
    private readonly NamespacedKey _keyOfRelevantSkin;

    public DynamicItemSkinModifier(NamespacedKey keyOfRelevantSkin)
    {
        _keyOfRelevantSkin = keyOfRelevantSkin;
    }

    public NamespacedKey Key => DuskKeys.DynamicItemSkin;

    public WeightModifierPhase Phase => WeightModifierPhase.Base;

    public int Priority => 0;

    public bool CanApply(WeightContext context)
    {
        if (!context.TryGet(DuskWeightContextKeys.ItemInfo, out DawnItemInfo? itemInfo))
            return false;

        if (itemInfo != context.Owner)
            return false;

        return true;
    }

    public void Apply(ref int value, WeightContext context)
    {
        if (!context.TryGet(DuskWeightContextKeys.ItemInfo, out DawnItemInfo? itemInfo))
        {
            // ItemInfo not found, shouldn't happen though.
            return;
        }

        if (!itemInfo.CustomData.TryGet(DuskKeys.EntityReplacements, out List<DuskEntityReplacementDefinition>? replacements))
        {
            // No skins found, shouldn't happen though.
            return;
        }

        int totalWeightOfAllReplacementsExceptRelevant = 0;
        foreach (DuskEntityReplacementDefinition replacementDefinition in replacements)
        {
            if (replacementDefinition.Key == _keyOfRelevantSkin)
            {
                // Skin trying to grab weight from the skin we applied this weightmodifier to.
                continue;
            }

            totalWeightOfAllReplacementsExceptRelevant += replacementDefinition.GetRarity(context.Moon, context.Dungeon, context.Weather, false);
        }

        // Equalise weight of this item's skin to be the same as the other item skins combined.
        value = totalWeightOfAllReplacementsExceptRelevant;
    }
}