using System;
using System.Collections.Generic;
using Dawn;
using Dusk.Weights;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Video;

namespace Dusk;

[Flags]
public enum SpawnTable
{
    Inside = 1 << 0,
    Outside = 1 << 1,
    Daytime = 1 << 2,
    Weed = 1 << 3,
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
    public VideoClip BestiaryVideo { get; private set; }
    [field: SerializeField]
    public string BestiaryWordOverride { get; private set; } = string.Empty;

    [field: Space(10)]
    [field: Header("Configs | Spawn Weights")]
    [field: SerializeField]
    public List<NamespacedConfigWeight> MoonSpawnWeightsConfig { get; private set; } = new();
    [field: SerializeField]
    public List<NamespacedConfigWeight> InteriorSpawnWeightsConfig { get; private set; } = new();
    [field: SerializeField]
    public List<NamespacedConfigWeight> WeatherSpawnWeightsConfig { get; private set; } = new();
    [field: SerializeField]
    public List<IntComparisonConfigWeight> RouteSpawnWeightsConfig { get; private set; } = new();

    [field: SerializeField]
    public bool GenerateSpawnWeightsConfig { get; private set; } = true;
    [field: SerializeField]
    public bool GeneratePowerLevelConfig { get; private set; } = true;
    [field: SerializeField]
    public bool GenerateMaxSpawnCountConfig { get; private set; } = true;

    private IWeightModifierSource<int> _spawnWeightSource = null!;
    public EnemyConfig Config { get; private set; }

    public override void Register(DuskMod mod)
    {
        base.Register(mod);
        using ConfigContext section = mod.ConfigManager.CreateConfigSectionForBundleData(AssetBundleData);
        Config = CreateEnemyConfig(section);
        BaseConfig = Config;

        if (GeneratePowerLevelConfig && Config.PowerLevel is { } ConfigPowerLevel)
        {
            EnemyType.PowerLevel = ConfigPowerLevel.Value;
        }
        if (GenerateMaxSpawnCountConfig && Config.MaxSpawnCount is { } ConfigMaxSpawnCount)
        {
            EnemyType.MaxCount = ConfigMaxSpawnCount.Value;
        }

        _spawnWeightSource = CreateSpawnWeightSource(
            () => GetConfigWeights(Config.MoonSpawnWeights, MoonSpawnWeightsConfig),
            () => GetConfigWeights(Config.InteriorSpawnWeights, InteriorSpawnWeightsConfig),
            () => GetConfigWeights(Config.WeatherSpawnWeights, WeatherSpawnWeightsConfig),
            () => GetConfigWeights(Config.RouteSpawnWeights, RouteSpawnWeightsConfig),
            () => 0);

        DawnLib.DefineEnemy(TypedKey, EnemyType, builder =>
        {
            if (SpawnTable.HasFlag(SpawnTable.Daytime))
            {
                builder.DefineDaytime(daytimeBuilder =>
                {
                    daytimeBuilder.SetWeights(weightProfile => weightProfile.AddSource(_spawnWeightSource));
                });
            }

            if (SpawnTable.HasFlag(SpawnTable.Outside))
            {
                builder.DefineOutside(outsideBuilder =>
                {
                    outsideBuilder.SetWeights(weightProfile => weightProfile.AddSource(_spawnWeightSource));
                });
            }

            if (SpawnTable.HasFlag(SpawnTable.Inside))
            {
                builder.DefineInside(insideBuilder =>
                {
                    insideBuilder.SetWeights(weightProfile => weightProfile.AddSource(_spawnWeightSource));
                });
            }

            if (SpawnTable.HasFlag(SpawnTable.Weed))
            {
                builder.DefineWeed(weedBuilder =>
                {
                    weedBuilder.SetWeights(weightProfile => weightProfile.AddSource(_spawnWeightSource));
                });
            }

            if (!string.IsNullOrWhiteSpace(BestiaryNodeText))
            {
                builder.CreateBestiaryNode(BestiaryNodeText);
                builder.CreateNameKeyword(BestiaryWordOverride);
                builder.SetBestiaryVideo(BestiaryVideo);
            }

            ApplyTagsTo(builder);
        });
    }

    public EnemyConfig CreateEnemyConfig(ConfigContext section)
    {
        EnemyConfig enemyConfig = new(section, EntityNameReference)
        {
            MoonSpawnWeights = GenerateSpawnWeightsConfig ? section.Bind($"{EntityNameReference} | Preset Moon Weights", $"Preset moon weights for {EntityNameReference}.", MoonSpawnWeightsConfig.ConvertManyToString()) : null,
            InteriorSpawnWeights = GenerateSpawnWeightsConfig ? section.Bind($"{EntityNameReference} | Preset Interior Weights", $"Preset interior weights for {EntityNameReference}.", InteriorSpawnWeightsConfig.ConvertManyToString()) : null,
            WeatherSpawnWeights = GenerateSpawnWeightsConfig ? section.Bind($"{EntityNameReference} | Preset Weather Weights", $"Preset weather weights for {EntityNameReference}.", WeatherSpawnWeightsConfig.ConvertManyToString()) : null,
            RouteSpawnWeights = GenerateSpawnWeightsConfig ? section.Bind($"{EntityNameReference} | Preset Route Weights", $"Preset route weights for {EntityNameReference}.", IntComparisonConfigWeight.ConvertManyToString(RouteSpawnWeightsConfig)) : null,

            PowerLevel = GeneratePowerLevelConfig ? section.Bind($"{EntityNameReference} | Power Level", $"Power level for {EntityNameReference}.", EnemyType.PowerLevel) : null,
            MaxSpawnCount = GenerateMaxSpawnCountConfig ? section.Bind($"{EntityNameReference} | Max Spawn Count", $"Max spawn count for {EntityNameReference}.", EnemyType.MaxCount) : null,
        };

        if (!enemyConfig.UserAllowedToEdit())
        {
            DuskBaseConfig.AssignValueIfNotNull(enemyConfig.MoonSpawnWeights, MoonSpawnWeightsConfig.ConvertManyToString());
            DuskBaseConfig.AssignValueIfNotNull(enemyConfig.InteriorSpawnWeights, InteriorSpawnWeightsConfig.ConvertManyToString());
            DuskBaseConfig.AssignValueIfNotNull(enemyConfig.WeatherSpawnWeights, WeatherSpawnWeightsConfig.ConvertManyToString());
            DuskBaseConfig.AssignValueIfNotNull(enemyConfig.RouteSpawnWeights, IntComparisonConfigWeight.ConvertManyToString(RouteSpawnWeightsConfig));

            DuskBaseConfig.AssignValueIfNotNull(enemyConfig.PowerLevel, EnemyType.PowerLevel);
            DuskBaseConfig.AssignValueIfNotNull(enemyConfig.MaxSpawnCount, EnemyType.MaxCount);
        }

        return enemyConfig;
    }

    public override void TryNetworkRegisterAssets()
    {
        if (!EnemyType.enemyPrefab.TryGetComponent(out NetworkObject _))
            return;

        DawnLib.RegisterNetworkPrefab(EnemyType.enemyPrefab);
    }

    protected override string EntityNameReference => EnemyType?.enemyName ?? string.Empty;
}