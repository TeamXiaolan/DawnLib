using System;
using System.Collections.Generic;
using Dawn;
using Dawn.Utils;
using Dusk.Utils;
using Dusk.Weights;
using UnityEngine;

namespace Dusk;

[CreateAssetMenu(fileName = "New Dungeon Definition", menuName = $"{DuskModConstants.Definitions}/Dungeon Definition")]
public class DuskDungeonDefinition : DuskContentDefinition<DawnDungeonInfo>
{
    [field: SerializeField]
    public DungeonFlowReference DungeonFlowReference { get; private set; }
    [field: SerializeField]
    public AudioClip? StingerAudio { get; private set; }

    [field: Space(5)]
    [field: Header("Configs | Spawn Weights")]
    [field: SerializeField]
    public List<NamespacedConfigWeight> MoonSpawnWeightsConfig { get; private set; } = new();
    [field: SerializeField]
    public List<NamespacedConfigWeight> WeatherSpawnWeightsConfig { get; private set; } = new();
    [field: SerializeField]
    public List<IntComparisonConfigWeight> RouteSpawnWeightsConfig { get; private set; } = new();
    [field: SerializeField]
    public bool GenerateSpawnWeightsConfig { get; private set; } = true;

    [field: Space(2f)]
    [field: Header("Configs | Generation")]
    [field: SerializeField]
    public int ExtraScrapGeneration { get; private set; } = 0;
    [field: SerializeField]
    public bool GenerateExtraScrapConfig { get; private set; } = true;
    [field: SerializeField]
    public bool GenerateClampConfig { get; private set; } = true;

    [field: Header("Configs | Misc")]
    [field: SerializeField]
    public BoundedRange DungeonRangeClamp { get; private set; } = new BoundedRange(0, 999);
    [field: SerializeField]
    public float MapTileSize { get; private set; } = 1f;
    [field: SerializeField]
    public bool StingerPlaysMoreThanOnce { get; private set; }
    [field: SerializeField]
    [field: Range(0, 100)]
    public float StingerPlayChance { get; private set; }

    public SpawnWeightsPreset SpawnWeights { get; private set; } = new();
    public DungeonConfig Config { get; private set; }

    public override void Register(DuskMod mod)
    {
        base.Register(mod);
        if (DungeonRangeClamp.Min == 0 && DungeonRangeClamp.Max == 0)
        {
            DungeonRangeClamp = new BoundedRange(0, 999);
        }

        using ConfigContext section = mod.ConfigManager.CreateConfigSectionForBundleData(AssetBundleData);
        Config = CreateDungeonConfig(section);
        BaseConfig = Config;

        List<UnresolvedNamespacedWeight> Moons = MoonSpawnWeightsConfig.ToUnresolvedWeights();
        if (Config.MoonSpawnWeights != null)
        {
            Moons = UnresolvedNamespacedWeight.ConvertManyFromString(Config.MoonSpawnWeights.Value);
        }

        List<UnresolvedNamespacedWeight> Weathers = WeatherSpawnWeightsConfig.ToUnresolvedWeights();
        if (Config.WeatherSpawnWeights != null)
        {
            Weathers = UnresolvedNamespacedWeight.ConvertManyFromString(Config.WeatherSpawnWeights.Value);
        }

        List<IntComparisonConfigWeight> Routes = RouteSpawnWeightsConfig;
        if (Config.RouteSpawnWeights != null)
        {
            Routes = IntComparisonConfigWeight.ConvertManyFromString(Config.RouteSpawnWeights.Value);
        }

        SpawnWeights.SetupSpawnWeightsPreset(Moons, [], Weathers);
        SpawnWeights.AddRule(new RoutePriceRule(new RoutePriceWeightTransformer(Routes)));
        DawnLib.DefineDungeon(TypedKey, DungeonFlowReference.FlowAssetName, builder =>
        {
            foreach (var mapping in DungeonFlowReference.ArchetypeTileSets)
            {
                builder.SetArchetypeTileSetMapping(mapping.ArchetypeName, mapping.TileSetNames);
            }
            builder.SetAssetBundlePath(mod.GetRelativePath("Assets", DungeonFlowReference.BundleName));
            builder.SetMapTileSize(MapTileSize);
            if (StingerAudio != null)
            {
                builder.SetFirstTimeAudio(StingerAudio);
            }
            builder.OverrideStingerPlaysMoreThanOnce(StingerPlaysMoreThanOnce);
            builder.OverrideStingerPlayChance(StingerPlayChance);
            builder.SetDungeonRangeClamp(DungeonRangeClamp);
            builder.SetExtraScrapGeneration(ExtraScrapGeneration);
            builder.SetWeights(weightBuilder => weightBuilder.SetGlobalWeight(SpawnWeights));
            ApplyTagsTo(builder);
        });
    }

    public DungeonConfig CreateDungeonConfig(ConfigContext section)
    {
        DungeonConfig dungeonConfig = new(section, EntityNameReference)
        {
            MoonSpawnWeights = GenerateSpawnWeightsConfig ? section.Bind($"{EntityNameReference} | Preset Moon Weights", $"Preset moon weights for {EntityNameReference}.", MoonSpawnWeightsConfig.ConvertManyToString()) : null,
            WeatherSpawnWeights = GenerateSpawnWeightsConfig ? section.Bind($"{EntityNameReference} | Preset Weather Weights", $"Preset weather weights for {EntityNameReference}.", WeatherSpawnWeightsConfig.ConvertManyToString()) : null,
            RouteSpawnWeights = GenerateSpawnWeightsConfig ? section.Bind($"{EntityNameReference} | Preset Route Weights", $"Preset route weights for {EntityNameReference}.", IntComparisonConfigWeight.ConvertManyToString(RouteSpawnWeightsConfig)) : null,

            ExtraScrapGeneration = GenerateExtraScrapConfig ? section.Bind($"{EntityNameReference} | Extra Scrap Generation", $"Extra scrap generation for {EntityNameReference}.", ExtraScrapGeneration) : null,
            DungeonRangeClamp = GenerateClampConfig ? section.Bind($"{EntityNameReference} | Dungeon Range Clamp", $"Dungeon range clamp for {EntityNameReference}.", DungeonRangeClamp) : null
        };

        if (!dungeonConfig.UserAllowedToEdit())
        {
            DuskBaseConfig.AssignValueIfNotNull(dungeonConfig.MoonSpawnWeights, MoonSpawnWeightsConfig.ConvertManyToString());
            DuskBaseConfig.AssignValueIfNotNull(dungeonConfig.WeatherSpawnWeights, WeatherSpawnWeightsConfig.ConvertManyToString());
            DuskBaseConfig.AssignValueIfNotNull(dungeonConfig.RouteSpawnWeights, IntComparisonConfigWeight.ConvertManyToString(RouteSpawnWeightsConfig));

            DuskBaseConfig.AssignValueIfNotNull(dungeonConfig.ExtraScrapGeneration, ExtraScrapGeneration);
            DuskBaseConfig.AssignValueIfNotNull(dungeonConfig.DungeonRangeClamp, DungeonRangeClamp);
        }
        return dungeonConfig;
    }

    public override void TryNetworkRegisterAssets() { }
    protected override string EntityNameReference => DungeonFlowReference?.FlowAssetName ?? string.Empty;
}