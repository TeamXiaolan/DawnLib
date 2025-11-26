using System;
using System.Collections.Generic;
using Dawn;
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

    [field: Space(10)]
    [field: Header("Config | Weights")]
    [field: SerializeField]
    public List<NamespacedConfigWeight> MoonSpawnWeightsConfig { get; private set; } = new();
    [field: SerializeField]
    public List<NamespacedConfigWeight> WeatherSpawnWeightsConfig { get; private set; } = new();

    [field: Header("Config | Generation")]
    [field: SerializeField]
    public bool GenerateSpawnWeightsConfig { get; private set; } = true;

    [field: Header("Config | Misc")]
    [field: SerializeField]
    public float MapTileSize { get; private set; }
    [field: SerializeField]
    public bool StingerPlaysMoreThanOnce { get; private set; }
    [field: SerializeField]
    public float StingerPlayChance { get; private set; }

    [field: Header("Config | Obsolete")]
    [field: SerializeField]
    [Obsolete]
    public string MoonSpawnWeights { get; private set; }
    [field: SerializeField]
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
        DawnLib.DefineDungeon(TypedKey, builder =>
        {
            builder.SetAssetBundlePath(mod.GetRelativePath("Assets", DungeonFlowReference.BundleName));
            builder.SetMapTileSize(MapTileSize);
            builder.SetFirstTimeAudio(StingerAudio);
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
        };
    }

    protected override string EntityNameReference => DungeonFlowReference?.FlowAssetName ?? string.Empty;
}