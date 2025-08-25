
namespace CodeRebirthLib;

public static class LethalContent
{
    public static TaggedRegistry<CRItemInfo> Items = new();
    public static TaggedRegistry<CREnemyInfo> Enemies = new();
    public static TaggedRegistry<CRMapObjectInfo> MapObjects = new();
    public static TaggedRegistry<CRMoonInfo> Moons = new();
    public static TaggedRegistry<CRTileSetInfo> TileSets = new();
    public static TaggedRegistry<CRDungeonInfo> Dungeons = new();
    public static TaggedRegistry<CRUnlockableItemInfo> Unlockables = new();
    public static TaggedRegistry<CRWeatherEffectInfo> Weathers = new();
}

public static class CRLibTags
{
    internal static readonly NamespacedKey IsExternal = NamespacedKey.From("code_rebirth_lib", "is_external");
}