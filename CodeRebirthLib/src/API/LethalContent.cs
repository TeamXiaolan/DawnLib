using CodeRebirthLib.Dungeons;

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
    public static Registry<CRWeatherInfo> Weathers = new();
}

public static class WeatherKeys
{
    public static readonly NamespacedKey<CRWeatherInfo> Rainy = NamespacedKey<CRWeatherInfo>.Vanilla("rainy");
}

public static class UnlockableItemKeys
{
    public static readonly NamespacedKey<CRUnlockableItemInfo> Teleporter = NamespacedKey<CRUnlockableItemInfo>.Vanilla("teleporter");
}

public static class ItemKeys
{
    public static readonly NamespacedKey<CRItemInfo> Flashlight = NamespacedKey<CRItemInfo>.Vanilla("flashlight");
    public static readonly NamespacedKey<CRItemInfo> ProFlashlight = NamespacedKey<CRItemInfo>.Vanilla("pro_flashlight");
    public static readonly NamespacedKey<CRItemInfo> BigAxle = NamespacedKey<CRItemInfo>.Vanilla("big_axle");
    public static readonly NamespacedKey<CRItemInfo> MetalSheet = NamespacedKey<CRItemInfo>.Vanilla("metal_sheet");
}

public static class MoonKeys
{
    public static readonly NamespacedKey<CRMoonInfo> Vow = NamespacedKey<CRMoonInfo>.Vanilla("vow");
    public static readonly NamespacedKey<CRMoonInfo> March = NamespacedKey<CRMoonInfo>.Vanilla("march");
}

public static class EnemyKeys
{
    public static readonly NamespacedKey<CREnemyInfo> Centipede = NamespacedKey<CREnemyInfo>.Vanilla("centipede");
    public static readonly NamespacedKey<CREnemyInfo> BunkerSpider = NamespacedKey<CREnemyInfo>.Vanilla("bunker_spider");
    public static readonly NamespacedKey<CREnemyInfo> Hoardingbug = NamespacedKey<CREnemyInfo>.Vanilla("hoarding_bug");
    public static readonly NamespacedKey<CREnemyInfo> Flowerman = NamespacedKey<CREnemyInfo>.Vanilla("flowerman");
    public static readonly NamespacedKey<CREnemyInfo> Crawler = NamespacedKey<CREnemyInfo>.Vanilla("crawler");
    public static readonly NamespacedKey<CREnemyInfo> Blob = NamespacedKey<CREnemyInfo>.Vanilla("blob");
    public static readonly NamespacedKey<CREnemyInfo> GiantKiwi = NamespacedKey<CREnemyInfo>.Vanilla("giant_kiwi");
}

public static class TileSetKeys
{

}

public static class DungeonKeys
{
    public static readonly NamespacedKey<CRDungeonInfo> Facility = NamespacedKey<CRDungeonInfo>.Vanilla("facility");
    public static readonly NamespacedKey<CRDungeonInfo> Facility3Exits = NamespacedKey<CRDungeonInfo>.Vanilla("facility_3exits");
    public static readonly NamespacedKey<CRDungeonInfo> FacilityLong = NamespacedKey<CRDungeonInfo>.Vanilla("facility_long");
    public static readonly NamespacedKey<CRDungeonInfo> Mansion = NamespacedKey<CRDungeonInfo>.Vanilla("mansion");
    public static readonly NamespacedKey<CRDungeonInfo> Mines = NamespacedKey<CRDungeonInfo>.Vanilla("facility");
}

public static class Tags
{
    public static readonly NamespacedKey ForestTag = NamespacedKey.Vanilla("forest");
}