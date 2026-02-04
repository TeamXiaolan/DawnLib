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
    [field: Space(2f)]
    [field: SerializeField]
    public int ExtraScrapGeneration { get; private set; } = 0;

    [field: Header("Configs | Generation")]
    [field: SerializeField]
    public bool GenerateSpawnWeightsConfig { get; private set; } = true;
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

    [field: Header("Configs | Obsolete")]
    [field: SerializeField]
    [field: DontDrawIfEmpty]
    [Obsolete]
    public string MoonSpawnWeights { get; private set; }
    [field: SerializeField]
    [field: DontDrawIfEmpty]
    [Obsolete]
    public string WeatherSpawnWeights { get; private set; }

#pragma warning disable CS0612
    internal string MoonSpawnWeightsCompat => MoonSpawnWeights;
    internal string WeatherSpawnWeightsCompat => WeatherSpawnWeights;
#pragma warning restore CS0612

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

        List<NamespacedConfigWeight> Moons = NamespacedConfigWeight.ConvertManyFromString(Config.MoonSpawnWeights?.Value ?? MoonSpawnWeightsCompat);
        List<NamespacedConfigWeight> Weathers = NamespacedConfigWeight.ConvertManyFromString(Config.WeatherSpawnWeights?.Value ?? WeatherSpawnWeightsCompat);

        SpawnWeights.SetupSpawnWeightsPreset(Moons.Count > 0 ? Moons : MoonSpawnWeightsConfig, new(), Weathers.Count > 0 ? Weathers : WeatherSpawnWeightsConfig);
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
        DungeonConfig dungeonConfig = new(section, EntityNameReference);

        dungeonConfig.MoonSpawnWeights = GenerateSpawnWeightsConfig ? section.Bind($"{EntityNameReference} | Preset Moon Weights", $"Preset moon weights for {EntityNameReference}.", MoonSpawnWeightsConfig.Count > 0 ? NamespacedConfigWeight.ConvertManyToString(MoonSpawnWeightsConfig) : MoonSpawnWeightsCompat) : null;
        dungeonConfig.WeatherSpawnWeights = GenerateSpawnWeightsConfig ? section.Bind($"{EntityNameReference} | Preset Weather Weights", $"Preset weather weights for {EntityNameReference}.", WeatherSpawnWeightsConfig.Count > 0 ? NamespacedConfigWeight.ConvertManyToString(WeatherSpawnWeightsConfig) : WeatherSpawnWeightsCompat) : null;
        dungeonConfig.ExtraScrapGeneration = GenerateExtraScrapConfig ? section.Bind($"{EntityNameReference} | Extra Scrap Generation", $"Extra scrap generation for {EntityNameReference}.", ExtraScrapGeneration) : null;
        dungeonConfig.DungeonRangeClamp = GenerateClampConfig ? section.Bind($"{EntityNameReference} | Dungeon Range Clamp", $"Dungeon range clamp for {EntityNameReference}.", DungeonRangeClamp) : null;

        if (!dungeonConfig.UserAllowedToEdit())
        {
            DuskBaseConfig.AssignValueIfNotNull(dungeonConfig.MoonSpawnWeights, MoonSpawnWeightsConfig.Count > 0 ? NamespacedConfigWeight.ConvertManyToString(MoonSpawnWeightsConfig) : MoonSpawnWeightsCompat);
            DuskBaseConfig.AssignValueIfNotNull(dungeonConfig.WeatherSpawnWeights, WeatherSpawnWeightsConfig.Count > 0 ? NamespacedConfigWeight.ConvertManyToString(WeatherSpawnWeightsConfig) : WeatherSpawnWeightsCompat);

            DuskBaseConfig.AssignValueIfNotNull(dungeonConfig.ExtraScrapGeneration, ExtraScrapGeneration);
            DuskBaseConfig.AssignValueIfNotNull(dungeonConfig.DungeonRangeClamp, DungeonRangeClamp);
        }
        return dungeonConfig;
    }

    public override void TryNetworkRegisterAssets() { }
    protected override string EntityNameReference => DungeonFlowReference?.FlowAssetName ?? string.Empty;
}