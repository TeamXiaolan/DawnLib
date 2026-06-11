namespace Dawn;

public static partial class DawnKeys
{
    public const string Namespace = "dawn_lib";

    // Data Keys
    public static readonly NamespacedKey LastVersion = NamespacedKey.From(Namespace, "last_version");
    public static readonly NamespacedKey DawnSave = NamespacedKey.From(Namespace, "dawn_save");
    public static readonly NamespacedKey StingerPlayed = NamespacedKey.From(Namespace, "played_stinger_once_before");
    public static readonly NamespacedKey ShipItemsSaveData = NamespacedKey.From(Namespace, "ship_items_save_data");
    public static readonly NamespacedKey ShipUnlockablesSaveData = NamespacedKey.From(Namespace, "ship_unlockables_save_data");

    // Weight Channels
    public static readonly NamespacedKey EnemyRarity = NamespacedKey.From(Namespace, "enemy_rarity");
    public static readonly NamespacedKey DungeonRarity = NamespacedKey.From(Namespace, "dungeon_rarity");
    public static readonly NamespacedKey ScrapRarity = NamespacedKey.From(Namespace, "scrap_rarity");
    public static readonly NamespacedKey MapObjectSpawnCurve = NamespacedKey.From(Namespace, "map_object_spawn_curve");
    public static readonly NamespacedKey WeatherRarity = NamespacedKey.From(Namespace, "weather_rarity");
    public static readonly NamespacedKey MoonSceneRarity = NamespacedKey.From(Namespace, "moon_scene_rarity");

    // Modifiers
    public static readonly NamespacedKey DungeonIntWeight = NamespacedKey.From(Namespace, "dungeon_int_weight");
    public static readonly NamespacedKey GlobalCurve = NamespacedKey.From(Namespace, "global_curve");
    public static readonly NamespacedKey GlobalBaseInt = NamespacedKey.From(Namespace, "global_base_int");
    public static readonly NamespacedKey WeatherIntWeight = NamespacedKey.From(Namespace, "weather_int_weight");
    public static readonly NamespacedKey MoonBaseInt = NamespacedKey.From(Namespace, "moon_base_int");
    public static readonly NamespacedKey MoonCurve = NamespacedKey.From(Namespace, "moon_curve");
    public static readonly NamespacedKey MoonIntWeight = NamespacedKey.From(Namespace, "moon_int_weight");

    // Extras
    public static readonly NamespacedKey MoonSceneIntWeight = NamespacedKey.From(Namespace, "moon_scene_int_weight");
    public static readonly NamespacedKey MoonSceneContext = NamespacedKey.From(Namespace, "moon_scene_context");
}