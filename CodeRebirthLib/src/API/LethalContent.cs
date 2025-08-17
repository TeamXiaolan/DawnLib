namespace CodeRebirthLib;

public static class LethalContent
{
    public static Registry<CRItemInfo> Items = new();
    public static Registry<CREnemyInfo> Enemies = new();
    public static Registry<CRMapObjectInfo> MapObjects = new();
    public static Registry<CRMoonInfo> Moons = new();
    public static Registry<CRTileSetInfo> TileSets = new();
    public static Registry<CRDungeonInfo> Dungeons = new();
    public static Registry<CRUnlockableItemInfo> Unlockables = new();
    public static Registry<CRWeatherEffectInfo> Weathers = new();
}

public static class Tags
{
    public static readonly NamespacedKey ForestTag = NamespacedKey.Vanilla("forest");
}