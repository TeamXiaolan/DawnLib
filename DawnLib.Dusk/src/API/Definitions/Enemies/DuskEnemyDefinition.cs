using System;
using System.Collections.Generic;
using Dawn;
using Dusk.Weights;
using Unity.Netcode;
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
    [field: SerializeField]
    public List<NamespacedConfigWeight> MoonSpawnWeightsConfig { get; private set; } = new();
    [field: SerializeField]
    public List<NamespacedConfigWeight> InteriorSpawnWeightsConfig { get; private set; } = new();
    [field: SerializeField]
    public List<NamespacedConfigWeight> WeatherSpawnWeightsConfig { get; private set; } = new();

    [field: SerializeField]
    public bool GenerateSpawnWeightsConfig { get; private set; } = true;

    [field: Header("Configs | Obsolete")]
    [field: SerializeField]
    [field: DontDrawIfEmpty]
    [Obsolete]
    public string MoonSpawnWeights { get; private set; }
    [field: SerializeField]
    [field: DontDrawIfEmpty]
    [Obsolete]
    public string InteriorSpawnWeights { get; private set; }
    [field: SerializeField]
    [field: DontDrawIfEmpty]
    [Obsolete]
    public string WeatherSpawnWeights { get; private set; }

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

        List<NamespacedConfigWeight> Moons = NamespacedConfigWeight.ConvertManyFromString(Config.MoonSpawnWeights?.Value ?? MoonSpawnWeights);
        List<NamespacedConfigWeight> Interiors = NamespacedConfigWeight.ConvertManyFromString(Config.InteriorSpawnWeights?.Value ?? InteriorSpawnWeights);
        List<NamespacedConfigWeight> Weathers = NamespacedConfigWeight.ConvertManyFromString(Config.WeatherSpawnWeights?.Value ?? WeatherSpawnWeights);

        SpawnWeights.SetupSpawnWeightsPreset(Moons.Count > 0 ? Moons : MoonSpawnWeightsConfig, Interiors.Count > 0 ? Interiors : InteriorSpawnWeightsConfig, Weathers.Count > 0 ? Weathers : WeatherSpawnWeightsConfig);

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

            if (!string.IsNullOrWhiteSpace(BestiaryNodeText))
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
            MoonSpawnWeights = GenerateSpawnWeightsConfig ? section.Bind($"{EntityNameReference} | Preset Moon Weights", $"Preset moon weights for {EntityNameReference}.", MoonSpawnWeightsConfig.Count > 0 ? NamespacedConfigWeight.ConvertManyToString(MoonSpawnWeightsConfig) : MoonSpawnWeights) : null,
            InteriorSpawnWeights = GenerateSpawnWeightsConfig ? section.Bind($"{EntityNameReference} | Preset Interior Weights", $"Preset interior weights for {EntityNameReference}.", InteriorSpawnWeightsConfig.Count > 0 ? NamespacedConfigWeight.ConvertManyToString(InteriorSpawnWeightsConfig) : InteriorSpawnWeights) : null,
            WeatherSpawnWeights = GenerateSpawnWeightsConfig ? section.Bind($"{EntityNameReference} | Preset Weather Weights", $"Preset weather weights for {EntityNameReference}.", WeatherSpawnWeightsConfig.Count > 0 ? NamespacedConfigWeight.ConvertManyToString(WeatherSpawnWeightsConfig) : WeatherSpawnWeights) : null,

            PowerLevel = section.Bind($"{EntityNameReference} | Power Level", $"Power level for {EntityNameReference}.", EnemyType.PowerLevel),
            MaxSpawnCount = section.Bind($"{EntityNameReference} | Max Spawn Count", $"Max spawn count for {EntityNameReference}.", EnemyType.MaxCount),
        };
    }

    public override void TryNetworkRegisterAssets()
    {
        if (!EnemyType.enemyPrefab.TryGetComponent(out NetworkObject _))
            return;

        DawnLib.RegisterNetworkPrefab(EnemyType.enemyPrefab);
    }

    protected override string EntityNameReference => EnemyType?.enemyName ?? string.Empty;
}