using Dawn;

namespace Dusk;

public static partial class DuskKeys
{
    public const string Namespace = "dawn_lib";

    // Data Keys
    public static readonly NamespacedKey EntityReplacements = NamespacedKey.From(Namespace, "entity_replacements");

    // Modifiers
    public static readonly NamespacedKey MapObjectSpawnMechanics = NamespacedKey.From(Namespace, "map_object_spawn_mechanics");
    public static readonly NamespacedKey RoutePriceIntWeight = NamespacedKey.From(Namespace, "route_price_int_weight");

    // Weight Channels
    public static readonly NamespacedKey EntityReplacementRarity = NamespacedKey.From(Namespace, "entity_replacement_rarity");

    // Context
    public static readonly NamespacedKey RoutePriceContext = NamespacedKey.From(Namespace, "route_price_context");
}