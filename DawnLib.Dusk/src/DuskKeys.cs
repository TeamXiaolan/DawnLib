using Dawn;

namespace Dusk;

public static partial class DuskKeys
{
    public const string Namespace = "dusk_mod";

    // Data Keys
    public static readonly NamespacedKey EntityReplacements = NamespacedKey.From(Namespace, "entity_replacements");

    // Modifiers
    public static readonly NamespacedKey MapObjectSpawnMechanics = NamespacedKey.From(Namespace, "map_object_spawn_mechanics");
    public static readonly NamespacedKey RoutePriceIntWeight = NamespacedKey.From(Namespace, "route_price_int_weight");
    public static readonly NamespacedKey DynamicItemSkin = NamespacedKey.From(Namespace, "dynamic_item_skin");
    public static readonly NamespacedKey RoutePriceCurveWeight = NamespacedKey.From(Namespace, "route_price_curve_weight");

    // Weight Channels
    public static readonly NamespacedKey EntityReplacementRarity = NamespacedKey.From(Namespace, "entity_replacement_rarity");

    // Context
    public static readonly NamespacedKey RoutePriceContext = NamespacedKey.From(Namespace, "route_price_context");
    public static readonly NamespacedKey ItemInfoContext = NamespacedKey.From(Namespace, "item_info_context");
}