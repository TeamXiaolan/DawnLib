
namespace Dawn;

public static class LethalContent
{
    public static TaggedRegistry<DawnItemInfo> Items = new();
    public static TaggedRegistry<DawnEnemyInfo> Enemies = new();
    public static TaggedRegistry<DawnMapObjectInfo> MapObjects = new();
    public static TaggedRegistry<DawnMoonInfo> Moons = new();
    public static Registry<DawnTileSetInfo> TileSets = new();
    public static TaggedRegistry<DawnDungeonInfo> Dungeons = new();
    public static TaggedRegistry<DawnUnlockableItemInfo> Unlockables = new();
    public static TaggedRegistry<DawnWeatherEffectInfo> Weathers = new();
    public static Registry<DawnArchetypeInfo> Archetypes = new();
    public static Registry<DawnStoryLogInfo> StoryLogs = new();
}

public static class DawnLibTags
{
    public static readonly NamespacedKey IsExternal = NamespacedKey.From("dawn_lib", "is_external");
    public static readonly NamespacedKey LunarConfig = NamespacedKey.From("dawn_lib", "lunar_config");
    public static readonly NamespacedKey HasBuyingPercent = NamespacedKey.From("dawn_lib", "has_buying_percent");
}