using System.Collections.Generic;
using DunGen.Graph;
using DunGen;
using Dawn.Utils;

namespace Dawn;

public sealed class DawnDungeonInfo : DawnBaseInfo<DawnDungeonInfo>
{
    internal DawnDungeonInfo(NamespacedKey<DawnDungeonInfo> key, HashSet<NamespacedKey> tags, DungeonFlow dungeonFlow, DawnWeightedValue<int> rarity, float mapTileSize, DawnStingerDetail stingerDetail, string assetBundlePath, BoundedRange dungeonClampRange, int extraScrapGeneration, IDataContainer? customData) : base(key, tags, customData)
    {
        DungeonFlow = dungeonFlow;
        Rarity = rarity;
        MapTileSize = mapTileSize;
        StingerDetail = stingerDetail;
        AssetBundlePath = assetBundlePath;
        DungeonClampRange = dungeonClampRange;
        ExtraScrapGeneration = extraScrapGeneration;
    }

    public DawnWeightedValue<int> Rarity { get; }

    public int GetRarity(DawnMoonInfo? moonInfo = null, DawnWeatherEffectInfo? weatherEffectInfo = null, bool resolveAutomatically = true)
    {
        return Rarity.GetValue(new WeightQuery
        {
            Subject = this,
            Moon = moonInfo,
            Weather = weatherEffectInfo,
            Channel = DawnWeightChannels.ScrapRarity.Key,
            ResolveAutomatically = resolveAutomatically
        });
    }

    public DungeonFlow DungeonFlow { get; }
    public string AssetBundlePath { get; }
    public float MapTileSize { get; private set; }
    public DawnStingerDetail StingerDetail { get; private set; }
    public BoundedRange DungeonClampRange { get; private set; }
    public int ExtraScrapGeneration { get; private set; }
    public HashSet<SpawnSyncedObject> SpawnSyncedObjects => CollectAllSpawnSyncedObjects();

    private HashSet<SpawnSyncedObject> CollectAllSpawnSyncedObjects()
    {
        HashSet<SpawnSyncedObject> result = new();
        foreach (TileSet tileSet in DungeonFlow.GetUsedTileSets())
        {
            if (tileSet.DawnInfo == null)
            {
                DawnPlugin.Logger.LogWarning("TileSet has no DawnInfo: " + tileSet.name + ", falling back to manual method.");
                result.UnionWith(DawnTileSetInfo.GrabAllSpawnSyncedObjects(tileSet));
                continue;
            }
            result.UnionWith(tileSet.DawnInfo.SpawnSyncedObjects);
        }
        return result;
    }

    public string GetPublicName()
    {
        return Key.Key.RemoveLeadingNumbers().ToCapitalized().ReplaceNumbersWithWords().Replace(" ", "_");
    }

    public const int FireExitGlobalPropID = 1231;
}