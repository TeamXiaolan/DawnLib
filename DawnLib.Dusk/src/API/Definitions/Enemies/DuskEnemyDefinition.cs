using System;
using Dawn;
using Dusk.Weights;
using UnityEngine;
using UnityEngine.Serialization;

namespace Dusk;

[Flags]
public enum SpawnTable
{
    Inside = 1 << 0,
    Outside = 1 << 1,
    Daytime = 1 << 2,
}

[CreateAssetMenu(fileName = "New Enemy Definition", menuName = $"{DuskModConstants.Definitions}/Enemy Definition")]
public class DuskEnemyDefinition : DuskContentDefinition<DawnEnemyInfo>
{
    [field: FormerlySerializedAs("enemyType")]
    [field: SerializeField]
    public EnemyType EnemyType { get; private set; }

    [field: SerializeField]
    public SpawnTable SpawnTable { get; private set; }

    [field: Header("Optional | Bestiary")]
    [field: TextArea(2, 20)]
    [field: SerializeField]
    public string BestiaryNodeText { get; private set; } = string.Empty;

    [field: SerializeField]
    public string BestiaryWordOverride { get; private set; } = string.Empty;

    [field: Space(10)]
    [field: Header("Configs | Spawn Weights | Format: <Namespace>:<Key>=<Operation><Value>, i.e. magic_wesleys_mod:trite=+20")]
    [field: TextArea(1, 10)]
    [field: SerializeField]
    public string MoonSpawnWeights { get; private set; } = "Vanilla=+0, Custom=+0, Valley=+0, Canyon=+0, Tundra=+0, Marsh=+0, Military=+0, Rocky=+0,Amythest=+0, Experimentation=+0, Assurance=+0, Vow=+0, Offense=+0, March=+0, Adamance=+0, Rend=+0, Dine=+0, Titan=+0, Artifice=+0, Embrion=+0";
    [field: TextArea(1, 10)]
    [field: SerializeField]
    public string InteriorSpawnWeights { get; private set; } = "Facility=+0, Mansion=+0, Mineshaft=+0";
    [field: TextArea(1, 10)]
    [field: SerializeField]
    public string WeatherSpawnWeights { get; private set; } = "None=*1, DustClouds=*1, Rainy=*1, Stormy=*1, Foggy=*1, Flooded=*1, Eclipsed=*1";
    [field: SerializeField]
    public bool GenerateSpawnWeightsConfig { get; private set; } = true;

    public SpawnWeightsPreset SpawnWeights { get; private set; } = new();
    public EnemyConfig Config { get; private set; }

    public override void Register(DuskMod mod)
    {
        base.Register(mod);
        using ConfigContext section = mod.ConfigManager.CreateConfigSectionForBundleData(AssetBundleData);
        Config = CreateEnemyConfig(section);

        EnemyType enemy = EnemyType;
        enemy.MaxCount = Config.MaxSpawnCount.Value;
        enemy.PowerLevel = Config.PowerLevel.Value;

        SpawnWeights.SetupSpawnWeightsPreset(Config.MoonSpawnWeights?.Value ?? MoonSpawnWeights, Config.InteriorSpawnWeights?.Value ?? InteriorSpawnWeights, Config.WeatherSpawnWeights?.Value ?? WeatherSpawnWeights);

        DawnLib.DefineEnemy(TypedKey, EnemyType, builder =>
        {
            if (SpawnTable.HasFlag(SpawnTable.Daytime))
            {
                builder.DefineDaytime(daytimeBuilder =>
                {
                    daytimeBuilder.SetWeights(weightBuilder => weightBuilder.SetGlobalWeight(SpawnWeights));
                });
            }

            if (SpawnTable.HasFlag(SpawnTable.Outside))
            {
                builder.DefineOutside(outsideBuilder =>
                {
                    outsideBuilder.SetWeights(weightBuilder => weightBuilder.SetGlobalWeight(SpawnWeights));
                });
            }

            if (SpawnTable.HasFlag(SpawnTable.Inside))
            {
                builder.DefineInside(insideBuilder =>
                {
                    insideBuilder.SetWeights(weightBuilder => weightBuilder.SetGlobalWeight(SpawnWeights));
                });
            }

            if (!string.IsNullOrEmpty(BestiaryNodeText))
            {
                builder.CreateBestiaryNode(BestiaryNodeText);
                builder.CreateNameKeyword(BestiaryWordOverride);
            }

            ApplyTagsTo(builder);
        });
    }

    public EnemyConfig CreateEnemyConfig(ConfigContext section)
    {
        return new EnemyConfig
        {
            MoonSpawnWeights = GenerateSpawnWeightsConfig ? section.Bind($"{EntityNameReference} | Preset Moon Weights", $"Preset moon weights for {EntityNameReference}.", MoonSpawnWeights) : null,
            InteriorSpawnWeights = GenerateSpawnWeightsConfig ? section.Bind($"{EntityNameReference} | Preset Interior Weights", $"Preset interior weights for {EntityNameReference}.", InteriorSpawnWeights) : null,
            WeatherSpawnWeights = GenerateSpawnWeightsConfig ? section.Bind($"{EntityNameReference} | Preset Weather Weights", $"Preset weather weights for {EntityNameReference}.", WeatherSpawnWeights) : null,

            PowerLevel = section.Bind($"{EntityNameReference} | Power Level", $"Power level for {EntityNameReference}.", EnemyType.PowerLevel),
            MaxSpawnCount = section.Bind($"{EntityNameReference} | Max Spawn Count", $"Max spawn count for {EntityNameReference}.", EnemyType.MaxCount),
        };
    }

    protected override string EntityNameReference => EnemyType?.enemyName ?? string.Empty;
}