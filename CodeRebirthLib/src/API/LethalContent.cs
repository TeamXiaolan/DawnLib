namespace CodeRebirthLib;

public static class LethalContent
{
    public static Registry<CRItemInfo> Items = new();
    public static Registry<CREnemyInfo> Enemies = new();
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
    public static readonly NamespacedKey<CREnemyInfo> Crawler = NamespacedKey<CREnemyInfo>.Vanilla("crawler");
    public static readonly NamespacedKey<CREnemyInfo> Spider = NamespacedKey<CREnemyInfo>.Vanilla("sand_spider");
}