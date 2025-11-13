using Dawn;
using DunGen.Graph;
using Dusk.Weights;
using UnityEngine;

namespace Dusk;

[CreateAssetMenu(fileName = "New Dungeon Definition", menuName = $"{DuskModConstants.Definitions}/Dungeon Definition")]
public class DuskDungeonDefinition : DuskContentDefinition<DawnDungeonInfo>
{
    [field: SerializeField]
    public DungeonFlow DungeonFlow { get; private set; }
    [field: SerializeField]
    public AudioClip? StingerAudio { get; private set; }

    [field: Space(10)]
    [field: Header("Config | Weights")]
    [field: SerializeField]
    public string MoonSpawnWeights { get; private set; }
    [field: SerializeField]
    public string WeatherSpawnWeights { get; private set; }
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

    public SpawnWeightsPreset SpawnWeights { get; private set; } = new();
    public DungeonConfig Config { get; private set; }

    public override void Register(DuskMod mod)
    {
        base.Register(mod);
        using ConfigContext section = mod.ConfigManager.CreateConfigSectionForBundleData(AssetBundleData);
        Config = CreateDungeonConfig(section);

        SpawnWeights.SetupSpawnWeightsPreset(MoonSpawnWeights, string.Empty, WeatherSpawnWeights);
        DawnLib.DefineDungeon(TypedKey, DungeonFlow, builder =>
        {
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
            MoonSpawnWeights = GenerateSpawnWeightsConfig ? section.Bind($"{EntityNameReference} | Preset Moon Weights", $"Preset moon weights for {EntityNameReference}.", MoonSpawnWeights) : null,
            WeatherSpawnWeights = GenerateSpawnWeightsConfig ? section.Bind($"{EntityNameReference} | Preset Weather Weights", $"Preset weather weights for {EntityNameReference}.", WeatherSpawnWeights) : null,
        };
    }

    protected override string EntityNameReference => DungeonFlow?.name ?? string.Empty;
}