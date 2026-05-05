using System;
using System.Collections.Generic;
using BepInEx.Configuration;
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
[HelpURL("https://thunderstore.io/c/lethal-company/p/TeamXiaolan/DawnLib/wiki/4100-c4-enemies/")]
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
    public List<IntComparisonConfigWeight> RouteSpawnWeightsConfig { get; private set; } = new();

    [field: SerializeField]
    public bool GenerateSpawnWeightsConfig { get; private set; } = true;
    [field: SerializeField]
    public bool GeneratePowerLevelConfig { get; private set; } = true;
    [field: SerializeField]
    public bool GenerateMaxSpawnCountConfig { get; private set; } = true;

    [field: SerializeField]
    [field: DontDrawIfEmpty("obsolete", "Obsolete")]
    [Obsolete]
    public string MoonSpawnWeights { get; private set; }
    [field: SerializeField]
    [field: DontDrawIfEmpty("obsolete", "Obsolete")]
    [Obsolete]
    public string InteriorSpawnWeights { get; private set; }
    [field: SerializeField]
    [field: DontDrawIfEmpty("obsolete", "Obsolete")]
    [Obsolete]
    public string WeatherSpawnWeights { get; private set; }

#pragma warning disable CS0612
    internal string MoonSpawnWeightsCompat => MoonSpawnWeights;
    internal string InteriorSpawnWeightsCompat => InteriorSpawnWeights;
    internal string WeatherSpawnWeightsCompat => WeatherSpawnWeights;
#pragma warning restore CS0612

    public SpawnWeightsPreset SpawnWeights { get; private set; } = new();
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

        List<NamespacedConfigWeight> Moons = NamespacedConfigWeight.ConvertManyFromString(MoonSpawnWeightsCompat);
        if (MoonSpawnWeightsConfig.Count > 0)
        {
            Moons = MoonSpawnWeightsConfig;
        }

        if (Config.MoonSpawnWeights != null)
        {
            Moons = NamespacedConfigWeight.ConvertManyFromString(Config.MoonSpawnWeights.Value);
        }

        List<NamespacedConfigWeight> Interiors = NamespacedConfigWeight.ConvertManyFromString(InteriorSpawnWeightsCompat);
        if (InteriorSpawnWeightsConfig.Count > 0)
        {
            Interiors = InteriorSpawnWeightsConfig;
        }

        if (Config.InteriorSpawnWeights != null)
        {
            Interiors = NamespacedConfigWeight.ConvertManyFromString(Config.InteriorSpawnWeights.Value);
        }

        List<NamespacedConfigWeight> Weathers = NamespacedConfigWeight.ConvertManyFromString(WeatherSpawnWeightsCompat);
        if (WeatherSpawnWeightsConfig.Count > 0)
        {
            Weathers = WeatherSpawnWeightsConfig;
        }

        if (Config.WeatherSpawnWeights != null)
        {
            Weathers = NamespacedConfigWeight.ConvertManyFromString(Config.WeatherSpawnWeights.Value);
        }

        List<IntComparisonConfigWeight> Routes = IntComparisonConfigWeight.ConvertManyFromString(string.Empty);
        if (RouteSpawnWeightsConfig.Count > 0)
        {
            Routes = RouteSpawnWeightsConfig;
        }

        if (Config.RouteSpawnWeights != null)
        {
            Routes = IntComparisonConfigWeight.ConvertManyFromString(Config.RouteSpawnWeights.Value);
        }

        SpawnWeights.SetupSpawnWeightsPreset(Moons, Interiors, Weathers);
        SpawnWeights.AddRule(new RoutePriceRule(new RoutePriceWeightTransformer(Routes)));
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
        EnemyConfig enemyConfig = new(section, EntityNameReference)
        {
            MoonSpawnWeights = GenerateSpawnWeightsConfig ? section.Bind($"{EntityNameReference} | Preset Moon Weights", $"Preset moon weights for {EntityNameReference}.", MoonSpawnWeightsConfig.Count > 0 ? NamespacedConfigWeight.ConvertManyToString(MoonSpawnWeightsConfig) : MoonSpawnWeightsCompat) : null,
            InteriorSpawnWeights = GenerateSpawnWeightsConfig ? section.Bind($"{EntityNameReference} | Preset Interior Weights", $"Preset interior weights for {EntityNameReference}.", InteriorSpawnWeightsConfig.Count > 0 ? NamespacedConfigWeight.ConvertManyToString(InteriorSpawnWeightsConfig) : InteriorSpawnWeightsCompat) : null,
            WeatherSpawnWeights = GenerateSpawnWeightsConfig ? section.Bind($"{EntityNameReference} | Preset Weather Weights", $"Preset weather weights for {EntityNameReference}.", WeatherSpawnWeightsConfig.Count > 0 ? NamespacedConfigWeight.ConvertManyToString(WeatherSpawnWeightsConfig) : WeatherSpawnWeightsCompat) : null,
            RouteSpawnWeights = GenerateSpawnWeightsConfig ? section.Bind($"{EntityNameReference} | Preset Route Weights", $"Preset route weights for {EntityNameReference}.", IntComparisonConfigWeight.ConvertManyToString(RouteSpawnWeightsConfig)) : null,

            PowerLevel = GeneratePowerLevelConfig ? section.Bind($"{EntityNameReference} | Power Level", $"Power level for {EntityNameReference}.", EnemyType.PowerLevel) : null,
            MaxSpawnCount = GenerateMaxSpawnCountConfig ? section.Bind($"{EntityNameReference} | Max Spawn Count", $"Max spawn count for {EntityNameReference}.", EnemyType.MaxCount) : null,
        };

        if (!enemyConfig.UserAllowedToEdit())
        {
            DuskBaseConfig.AssignValueIfNotNull(enemyConfig.MoonSpawnWeights, MoonSpawnWeightsConfig.Count > 0 ? NamespacedConfigWeight.ConvertManyToString(MoonSpawnWeightsConfig) : MoonSpawnWeightsCompat);
            DuskBaseConfig.AssignValueIfNotNull(enemyConfig.InteriorSpawnWeights, InteriorSpawnWeightsConfig.Count > 0 ? NamespacedConfigWeight.ConvertManyToString(InteriorSpawnWeightsConfig) : InteriorSpawnWeightsCompat);
            DuskBaseConfig.AssignValueIfNotNull(enemyConfig.WeatherSpawnWeights, WeatherSpawnWeightsConfig.Count > 0 ? NamespacedConfigWeight.ConvertManyToString(WeatherSpawnWeightsConfig) : WeatherSpawnWeightsCompat);
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