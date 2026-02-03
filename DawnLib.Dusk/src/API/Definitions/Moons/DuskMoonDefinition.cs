using System;
using System.Collections.Generic;
using Dawn;
using Dawn.Utils;
using Dusk.Utils;
using Dusk.Weights;
using LethalLib.Modules;
using UnityEngine;

namespace Dusk;

[CreateAssetMenu(fileName = "New Moon Definition", menuName = $"{DuskModConstants.Definitions}/Moon Definition")]
public class DuskMoonDefinition : DuskContentDefinition<DawnMoonInfo>
{
    [field: SerializeField]
    public SelectableLevel Level { get; private set; }

    [SerializeField]
    private List<DuskMoonSceneData> _scenes = [];

    [field: SerializeField]
    public DuskTerminalPredicate? TerminalPredicate { get; private set; }

    [field: SerializeField]
    public DuskPricingStrategy? PricingStrategy { get; private set; }

    [field: Header("Configs | Defaults")]
    [field: SerializeField]
    public int Cost { get; private set; }
    [field: SerializeField]
    [field: Tooltip("Vanilla typically hard codes this to a value of 3.")]
    public float OutsideEnemiesSpawnProbabilityRange { get; private set; } = 3;

    [field: Header("Configs | Generation")]
    [field: SerializeField]
    public bool GenerateEnemyPowerCountConfigs { get; private set; } = true;
    [field: SerializeField]
    public bool GenerateEnemySpawnCurveConfigs { get; private set; } = true;
    [field: SerializeField]
    public bool GenerateEnemySpawnProbabilityRangeConfigs { get; private set; } = true;
    [field: SerializeField]
    public bool GenerateMinMaxScrapConfig { get; private set; } = true;
    [field: SerializeField]
    public bool GenerateTimeConfig { get; private set; } = true;
    [field: SerializeField]
    public bool GenerateCostConfig { get; private set; } = true;
    [field: SerializeField]
    public bool GenerateDisableUnlockConfig { get; private set; } = true;
    [field: SerializeField]
    public bool GenerateDisablePricingStrategyConfig { get; private set; } = true;

    public MoonConfig Config { get; private set; }

    public override void Register(DuskMod mod)
    {
        base.Register(mod);
        using ConfigContext section = mod.ConfigManager.CreateConfigSectionForBundleData(AssetBundleData);
        Config = CreateMoonConfig(section);

        DawnLib.DefineMoon(TypedKey, Level, builder =>
        {
            foreach (DuskMoonSceneData sceneData in _scenes)
            {
                builder.AddScene(
                    sceneData.Key,
                    sceneData.ShipLandingOverrideAnimation,
                    sceneData.ShipTakeoffOverrideAnimation,
                    sceneData.Weight(),
                    mod.GetRelativePath("Assets", sceneData.BundleName),
                    sceneData.Scene.ScenePath
                );
            }

            bool disableUnlockRequirements = Config.DisableUnlockRequirements?.Value ?? false;
            if (!disableUnlockRequirements && TerminalPredicate)
            {
                TerminalPredicate.Register(TypedKey);
                builder.SetPurchasePredicate(TerminalPredicate);
            }

            bool disablePricingStrategy = Config.DisablePricingStrategy?.Value ?? false;
            if (!disablePricingStrategy && PricingStrategy)
            {
                PricingStrategy.Register(Key);
                builder.OverrideCost(PricingStrategy);
            }
            else
            {
                builder.OverrideCost(Config.Cost?.Value ?? Cost);
            }

            builder.OverrideTimeMultiplier(Config.TimeFactor?.Value ?? Level.DaySpeedMultiplier);
            builder.OverrideMinMaxScrap(new BoundedRange(Config.MinMaxScrap?.Value.Min ?? Level.minScrap, Config.MinMaxScrap?.Value.Max ?? Level.maxScrap));
            builder.OverrideEnemyPowerCount(Config.InsideEnemyPowerCount?.Value ?? Level.maxEnemyPowerCount, Config.OutsideEnemyPowerCount?.Value ?? Level.maxOutsideEnemyPowerCount, Config.DaytimeEnemyPowerCount?.Value ?? Level.maxDaytimeEnemyPowerCount);
            builder.OverrideEnemySpawnCurves(Config.InsideEnemySpawnCurve?.Value ?? Level.enemySpawnChanceThroughoutDay, Config.OutsideEnemySpawnCurve?.Value ?? Level.outsideEnemySpawnChanceThroughDay, Config.DaytimeEnemySpawnCurve?.Value ?? Level.daytimeEnemySpawnChanceThroughDay);
            builder.OverrideEnemySpawnRanges(Config.InsideEnemySpawnRange?.Value ?? Level.spawnProbabilityRange, Config.OutsideEnemySpawnRange?.Value ?? OutsideEnemiesSpawnProbabilityRange, Config.DaytimeEnemySpawnRange?.Value ?? Level.daytimeEnemiesProbabilityRange);
        });
    }

    public MoonConfig CreateMoonConfig(ConfigContext section)
    {
        MoonConfig moonConfig = new(section, EntityNameReference)
        {
            Cost = GenerateCostConfig ? section.Bind($"{EntityNameReference} | Cost", $"Cost for {EntityNameReference} in the shop.", Cost) : null,
            MinMaxScrap = GenerateMinMaxScrapConfig ? section.Bind($"{EntityNameReference} | Min/Max Scrap", $"Min/Max scrap for {EntityNameReference}.", new BoundedRange(Level.minScrap, Level.maxScrap)) : null,
            TimeFactor = GenerateTimeConfig && Level.spawnEnemiesAndScrap ? section.Bind($"{EntityNameReference} | Time Multiplier", $"Time multiplier for {EntityNameReference}.", Level.DaySpeedMultiplier) : null,

            InsideEnemyPowerCount = GenerateEnemyPowerCountConfigs && Level.spawnEnemiesAndScrap ? section.Bind($"{EntityNameReference} | Inside Enemy Power Count", $"Inside enemy power count for {EntityNameReference}.", Level.maxEnemyPowerCount) : null,
            OutsideEnemyPowerCount = GenerateEnemyPowerCountConfigs && Level.spawnEnemiesAndScrap ? section.Bind($"{EntityNameReference} | Outside Enemy Power Count", $"Outside enemy power count for {EntityNameReference}.", Level.maxOutsideEnemyPowerCount) : null,
            DaytimeEnemyPowerCount = GenerateEnemyPowerCountConfigs && Level.spawnEnemiesAndScrap ? section.Bind($"{EntityNameReference} | Daytime Enemy Power Count", $"Daytime enemy power count for {EntityNameReference}.", Level.maxDaytimeEnemyPowerCount) : null,

            InsideEnemySpawnRange = GenerateEnemySpawnProbabilityRangeConfigs && Level.spawnEnemiesAndScrap ? section.Bind($"{EntityNameReference} | Inside Enemy Spawn Range", $"Inside enemy spawn range for {EntityNameReference}.", Level.spawnProbabilityRange) : null,
            OutsideEnemySpawnRange = GenerateEnemySpawnProbabilityRangeConfigs && Level.spawnEnemiesAndScrap ? section.Bind($"{EntityNameReference} | Outside Enemy Spawn Range", $"Outside enemy spawn range for {EntityNameReference}.", OutsideEnemiesSpawnProbabilityRange) : null,
            DaytimeEnemySpawnRange = GenerateEnemySpawnProbabilityRangeConfigs && Level.spawnEnemiesAndScrap ? section.Bind($"{EntityNameReference} | Daytime Enemy Spawn Range", $"Daytime enemy spawn range for {EntityNameReference}.", Level.daytimeEnemiesProbabilityRange) : null,

            InsideEnemySpawnCurve = GenerateEnemySpawnCurveConfigs && Level.spawnEnemiesAndScrap ? section.Bind($"{EntityNameReference} | Inside Enemy Spawn Curve", $"Inside enemy spawn curve for {EntityNameReference}.", Level.enemySpawnChanceThroughoutDay) : null,
            OutsideEnemySpawnCurve = GenerateEnemySpawnCurveConfigs && Level.spawnEnemiesAndScrap ? section.Bind($"{EntityNameReference} | Outside Enemy Spawn Curve", $"Outside enemy spawn curve for {EntityNameReference}.", Level.outsideEnemySpawnChanceThroughDay) : null,
            DaytimeEnemySpawnCurve = GenerateEnemySpawnCurveConfigs && Level.spawnEnemiesAndScrap ? section.Bind($"{EntityNameReference} | Daytime Enemy Spawn Curve", $"Daytime enemy spawn curve for {EntityNameReference}.", Level.daytimeEnemySpawnChanceThroughDay) : null,

            DisableUnlockRequirements = GenerateDisableUnlockConfig && TerminalPredicate ? section.Bind($"{EntityNameReference} | Disable Unlock Requirements", $"Whether {EntityNameReference} should have it's unlock requirements disabled.", false) : null,
            DisablePricingStrategy = GenerateDisablePricingStrategyConfig && PricingStrategy ? section.Bind($"{EntityNameReference} | Disable Pricing Strategy", $"Whether {EntityNameReference} should have it's pricing strategy disabled.", false) : null,
        };

        if (!moonConfig.UserAllowedToEdit())
        {
            DuskBaseConfig.AssignValueIfNotNull(moonConfig.Cost, Cost);
            DuskBaseConfig.AssignValueIfNotNull(moonConfig.MinMaxScrap, new BoundedRange(Level.minScrap, Level.maxScrap));
            DuskBaseConfig.AssignValueIfNotNull(moonConfig.TimeFactor, Level.DaySpeedMultiplier);

            DuskBaseConfig.AssignValueIfNotNull(moonConfig.InsideEnemyPowerCount, Level.maxEnemyPowerCount);
            DuskBaseConfig.AssignValueIfNotNull(moonConfig.OutsideEnemyPowerCount, Level.maxOutsideEnemyPowerCount);
            DuskBaseConfig.AssignValueIfNotNull(moonConfig.DaytimeEnemyPowerCount, Level.maxDaytimeEnemyPowerCount);

            DuskBaseConfig.AssignValueIfNotNull(moonConfig.InsideEnemySpawnRange, Level.spawnProbabilityRange);
            DuskBaseConfig.AssignValueIfNotNull(moonConfig.OutsideEnemySpawnRange, OutsideEnemiesSpawnProbabilityRange);
            DuskBaseConfig.AssignValueIfNotNull(moonConfig.DaytimeEnemySpawnRange, Level.daytimeEnemiesProbabilityRange);

            DuskBaseConfig.AssignValueIfNotNull(moonConfig.InsideEnemySpawnCurve, Level.enemySpawnChanceThroughoutDay);
            DuskBaseConfig.AssignValueIfNotNull(moonConfig.OutsideEnemySpawnCurve, Level.outsideEnemySpawnChanceThroughDay);
            DuskBaseConfig.AssignValueIfNotNull(moonConfig.DaytimeEnemySpawnCurve, Level.daytimeEnemySpawnChanceThroughDay);

            DuskBaseConfig.AssignValueIfNotNull(moonConfig.DisableUnlockRequirements, false);
            DuskBaseConfig.AssignValueIfNotNull(moonConfig.DisablePricingStrategy, false);
        }
        return moonConfig;
    }

    public override void TryNetworkRegisterAssets() { }
    protected override string EntityNameReference => Level?.PlanetName ?? string.Empty;
}

[Serializable]
public class DuskMoonSceneData
{
    public SceneReference Scene;
    public string BundleName => Scene.BundleName;
    public string SceneName => Scene.SceneName;

    [InspectorName("Namespace"), DefaultKeySource("SceneName")]
    public NamespacedKey<IMoonSceneInfo> Key;

    [field: SerializeField]
    public AnimationClip ShipLandingOverrideAnimation { get; private set; }
    [field: SerializeField]
    public AnimationClip ShipTakeoffOverrideAnimation { get; private set; }

    [field: SerializeField]
    public int BaseWeight { get; private set; } = 100;
    [field: Header("Configs | SpawnWeights")]
    [field: SerializeField]
    public List<NamespacedConfigWeight> WeatherSpawnWeightsConfig { get; private set; } = new();

    [field: Header("Configs | Obsolete")]
    [field: SerializeField]
    [field: DontDrawIfEmpty]
    [Obsolete]
    public string WeatherWeights { get; private set; }

#pragma warning disable CS0612
    internal string WeatherWeightsCompat => WeatherWeights;
#pragma warning restore CS0612

    public SpawnWeightsPreset SpawnWeights { get; private set; } = new();
    private ProviderTable<int?, DawnMoonInfo, SpawnWeightContext> _weights;

    public ProviderTable<int?, DawnMoonInfo, SpawnWeightContext> Weight() // TODO: make this configurable
    {
        List<NamespacedConfigWeight> Weathers = NamespacedConfigWeight.ConvertManyFromString(WeatherWeightsCompat);

        SpawnWeights.SetupSpawnWeightsPreset(new(), new(), Weathers.Count > 0 ? Weathers : WeatherSpawnWeightsConfig, BaseWeight);
        WeightTableBuilder<DawnMoonInfo, SpawnWeightContext> builder = new WeightTableBuilder<DawnMoonInfo, SpawnWeightContext>();
        builder.SetGlobalWeight(SpawnWeights);
        _weights = builder.Build();
        return _weights;
    }
}