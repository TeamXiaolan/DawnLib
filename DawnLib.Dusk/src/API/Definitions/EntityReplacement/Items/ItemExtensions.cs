using Dawn.Preloader.Interfaces;

namespace Dusk;

public static class ItemExtensions
{
    public static DuskItemReplacementDefinition? GetItemReplacement(this Item item)
    {
        DuskItemReplacementDefinition? itemReplacementDefinition = (DuskItemReplacementDefinition?)((ICurrentEntityReplacement)item).CurrentEntityReplacement;
        return itemReplacementDefinition;
    }

    internal static bool HasItemReplacement(this Item item)
    {
        return item.GetItemReplacement() != null;
    }

    internal static void SetItemReplacement(this Item item, DuskItemReplacementDefinition itemReplacementDefinition)
    {
        ((ICurrentEntityReplacement)item).CurrentEntityReplacement = itemReplacementDefinition;
    }
}
