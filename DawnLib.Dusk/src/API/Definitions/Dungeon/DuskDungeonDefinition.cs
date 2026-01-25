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

    [field: Header("Configs | Generation")]
    [field: SerializeField]
    public bool GenerateSpawnWeightsConfig { get; private set; } = true;

    [field: Header("Configs | Misc")]
    [field: SerializeField]
    public BoundedRange DungeonRangeClamp { get; private set; }
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

    public SpawnWeightsPreset SpawnWeights { get; private set; } = new();
    public DungeonConfig Config { get; private set; }

    public override void Register(DuskMod mod)
    {
        base.Register(mod);
        using ConfigContext section = mod.ConfigManager.CreateConfigSectionForBundleData(AssetBundleData);
        Config = CreateDungeonConfig(section);

        List<NamespacedConfigWeight> Moons = NamespacedConfigWeight.ConvertManyFromString(Config.MoonSpawnWeights?.Value ?? MoonSpawnWeights);
        List<NamespacedConfigWeight> Weathers = NamespacedConfigWeight.ConvertManyFromString(Config.WeatherSpawnWeights?.Value ?? WeatherSpawnWeights);

        SpawnWeights.SetupSpawnWeightsPreset(Moons.Count > 0 ? Moons : MoonSpawnWeightsConfig, new(), Weathers.Count > 0 ? Weathers : WeatherSpawnWeightsConfig);
        DawnLib.DefineDungeon(TypedKey, DungeonFlowReference.FlowAssetName, builder =>
        {
            foreach (var mapping in DungeonFlowReference.ArchetypeTileSets)
            {
                builder.SetArchetypeTileSetMapping(mapping.ArchetypeName, mapping.TileSetNames);
            }
            builder.SetAssetBundlePath(mod.GetRelativePath("Assets", DungeonFlowReference.BundleName));
            builder.SetMapTileSize(MapTileSize);
            builder.SetFirstTimeAudio(StingerAudio);
            builder.OverrideStingerPlaysMoreThanOnce(StingerPlaysMoreThanOnce);
            builder.OverrideStingerPlayChance(StingerPlayChance);
            builder.SetDungeonRangeClamp(DungeonRangeClamp);
            builder.SetWeights(weightBuilder => weightBuilder.SetGlobalWeight(SpawnWeights));
            ApplyTagsTo(builder);
        });
    }

    public DungeonConfig CreateDungeonConfig(ConfigContext section)
    {
        return new DungeonConfig
        {
            MoonSpawnWeights = GenerateSpawnWeightsConfig ? section.Bind($"{EntityNameReference} | Preset Moon Weights", $"Preset moon weights for {EntityNameReference}.", MoonSpawnWeightsConfig.Count > 0 ? NamespacedConfigWeight.ConvertManyToString(MoonSpawnWeightsConfig) : MoonSpawnWeights) : null,
            WeatherSpawnWeights = GenerateSpawnWeightsConfig ? section.Bind($"{EntityNameReference} | Preset Weather Weights", $"Preset weather weights for {EntityNameReference}.", WeatherSpawnWeightsConfig.Count > 0 ? NamespacedConfigWeight.ConvertManyToString(WeatherSpawnWeightsConfig) : WeatherSpawnWeights) : null,
            DungeonRangeClamp = section.Bind($"{EntityNameReference} | Dungeon Range Clamp", $"Dungeon range clamp for {EntityNameReference}.", DungeonRangeClamp),
        };
    }

    public override void TryNetworkRegisterAssets() { }
    protected override string EntityNameReference => DungeonFlowReference?.FlowAssetName ?? string.Empty;
}