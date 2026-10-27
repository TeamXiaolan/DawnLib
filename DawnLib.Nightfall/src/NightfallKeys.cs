using Dawn;

namespace Nightfall;

public static partial class NightfallKeys
{
    public const string Namespace = "nightfall_mod";

    // Data Keys
    public static readonly NamespacedKey EntityReplacements = NamespacedKey.From(Namespace, "entity_replacements");

    // Modifiers
    public static readonly NamespacedKey DynamicItemSkin = NamespacedKey.From(Namespace, "dynamic_item_skin");

    // Weight Channels
    public static readonly NamespacedKey EntityReplacementRarity = NamespacedKey.From(Namespace, "entity_replacement_rarity");

    // Weight Contexts
    public static readonly NamespacedKey ItemInfoContext = NamespacedKey.From(Namespace, "item_info_context");
}