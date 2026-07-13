using System;
using System.Collections.Generic;
using System.Linq;
using Dawn;
using Dawn.Utils;
using Dusk.Utils;
using Dusk.Weights;
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

    [field: SerializeField]
    public int MaxDaytimeDiversityPowerCount { get; private set; } = 100;

    [field: SerializeField]
    public SpawnableEnemyWithRarity[]? WeedEnemies { get; private set; } = null; // Null before 1.0.0 DawnLib
    [field: SerializeField]
    public int MaxWeedDiversityPowerCount { get; private set; } = 100;
    [field: SerializeField]
    public int MaxWeedEnemyPowerCount { get; private set; } = 4;
    [field: SerializeField]
    public float WeedEnemiesProbabilityRange { get; private set; } = 1;
    [field: SerializeField]
    public AnimationCurve WeedEnemySpawnChanceThroughDay { get; private set; } = AnimationCurve.Constant(0f, 1f, 2f);

    [field: Header("Configs | Generation")]
    [field: SerializeField]
    public bool GenerateEnemyPowerCountConfigs { get; private set; } = true;
    [field: SerializeField]
    public bool GenerateEnemyDiversityCountConfigs { get; private set; } = true;
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
        if (WeedEnemies == null)
        {
            EnemyType blankKidnapperFox = EnemyType.CreateInstance<EnemyType>();
            blankKidnapperFox.name = "BushWolf";
            blankKidnapperFox.enemyName = "Bush Wolf";
            WeedEnemies =
            [
                new SpawnableEnemyWithRarity(blankKidnapperFox, 100)
            ];
        }

        using ConfigContext section = mod.ConfigManager.CreateConfigSectionForBundleData(AssetBundleData);
        Config = CreateMoonConfig(section);
        BaseConfig = Config;

        DawnLib.DefineMoon(TypedKey, Level, builder =>
        {
            foreach (DuskMoonSceneData sceneData in _scenes)
            {
                builder.AddScene(
                    sceneData.Key,
                    sceneData.ShipLandingOverrideAnimation,
                    sceneData.ShipTakeoffOverrideAnimation,
                    sceneData.Weight(section, _scenes.Count),
                    mod.GetRelativePath("Assets", sceneData.BundleName),
                    sceneData.Scene.ScenePath
                );
            }

            bool disableUnlockRequirements = Config.DisableUnlockRequirements?.Value ?? false;
            if (!disableUnlockRequirements && TerminalPredicate != null)
            {
                TerminalPredicate.Register(TypedKey);
                builder.SetPurchasePredicate(TerminalPredicate);
            }

            bool disablePricingStrategy = Config.DisablePricingStrategy?.Value ?? false;
            if (!disablePricingStrategy && PricingStrategy != null)
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
            builder.OverrideEnemyPowerCount(Config.InsideEnemyPowerCount?.Value ?? Level.maxEnemyPowerCount, Config.OutsideEnemyPowerCount?.Value ?? Level.maxOutsideEnemyPowerCount, Config.DaytimeEnemyPowerCount?.Value ?? Level.maxDaytimeEnemyPowerCount, Config.WeedEnemyPowerCount?.Value ?? MaxWeedEnemyPowerCount);
            builder.OverrideEnemySpawnCurves(Config.InsideEnemySpawnCurve?.Value ?? Level.enemySpawnChanceThroughoutDay, Config.OutsideEnemySpawnCurve?.Value ?? Level.outsideEnemySpawnChanceThroughDay, Config.DaytimeEnemySpawnCurve?.Value ?? Level.daytimeEnemySpawnChanceThroughDay, Config.WeedEnemySpawnCurve?.Value ?? WeedEnemySpawnChanceThroughDay);
            builder.OverrideEnemySpawnRanges(Config.InsideEnemySpawnRange?.Value ?? Level.spawnProbabilityRange, Config.OutsideEnemySpawnRange?.Value ?? OutsideEnemiesSpawnProbabilityRange, Config.DaytimeEnemySpawnRange?.Value ?? Level.daytimeEnemiesProbabilityRange, Config.WeedEnemySpawnRange?.Value ?? WeedEnemiesProbabilityRange);
            builder.OverrideDiversityPowerCounts(Config.InsideDiversityPowerCount?.Value ?? Level.maxInsideDiversityPowerCount, Config.OutsideDiversityPowerCount?.Value ?? Level.maxOutsideDiversityPowerCount, Config.DaytimeDiversityPowerCount?.Value ?? MaxDaytimeDiversityPowerCount, Config.WeedDiversityPowerCount?.Value ?? MaxWeedDiversityPowerCount);
            builder.SetWeedEnemies(WeedEnemies.ToList());
            ApplyTagsTo(builder);
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
            WeedEnemyPowerCount = GenerateEnemyPowerCountConfigs && Level.spawnEnemiesAndScrap ? section.Bind($"{EntityNameReference} | Weed Enemy Power Count", $"Weed enemy power count for {EntityNameReference}.", MaxWeedEnemyPowerCount) : null,

            InsideDiversityPowerCount = GenerateEnemyDiversityCountConfigs && Level.spawnEnemiesAndScrap ? section.Bind($"{EntityNameReference} | Inside Enemy Diversity Count", $"Inside enemy diversity count for {EntityNameReference}.", Level.maxInsideDiversityPowerCount) : null,
            OutsideDiversityPowerCount = GenerateEnemyDiversityCountConfigs && Level.spawnEnemiesAndScrap ? section.Bind($"{EntityNameReference} | Outside Enemy Diversity Count", $"Outside enemy diversity count for {EntityNameReference}.", Level.maxOutsideDiversityPowerCount) : null,
            DaytimeDiversityPowerCount = GenerateEnemyDiversityCountConfigs && Level.spawnEnemiesAndScrap ? section.Bind($"{EntityNameReference} | Daytime Enemy Diversity Count", $"Daytime enemy diversity count for {EntityNameReference}.", MaxDaytimeDiversityPowerCount) : null,
            WeedDiversityPowerCount = GenerateEnemyDiversityCountConfigs && Level.spawnEnemiesAndScrap ? section.Bind($"{EntityNameReference} | Weed Enemy Diversity Count", $"Weed enemy diversity count for {EntityNameReference}.", MaxWeedDiversityPowerCount) : null,

            InsideEnemySpawnRange = GenerateEnemySpawnProbabilityRangeConfigs && Level.spawnEnemiesAndScrap ? section.Bind($"{EntityNameReference} | Inside Enemy Spawn Range", $"Inside enemy spawn range for {EntityNameReference}.", Level.spawnProbabilityRange) : null,
            OutsideEnemySpawnRange = GenerateEnemySpawnProbabilityRangeConfigs && Level.spawnEnemiesAndScrap ? section.Bind($"{EntityNameReference} | Outside Enemy Spawn Range", $"Outside enemy spawn range for {EntityNameReference}.", OutsideEnemiesSpawnProbabilityRange) : null,
            DaytimeEnemySpawnRange = GenerateEnemySpawnProbabilityRangeConfigs && Level.spawnEnemiesAndScrap ? section.Bind($"{EntityNameReference} | Daytime Enemy Spawn Range", $"Daytime enemy spawn range for {EntityNameReference}.", Level.daytimeEnemiesProbabilityRange) : null,
            WeedEnemySpawnRange = GenerateEnemySpawnProbabilityRangeConfigs && Level.spawnEnemiesAndScrap ? section.Bind($"{EntityNameReference} | Weed Enemy Spawn Range", $"Weed enemy spawn range for {EntityNameReference}.", WeedEnemiesProbabilityRange) : null,

            InsideEnemySpawnCurve = GenerateEnemySpawnCurveConfigs && Level.spawnEnemiesAndScrap ? section.Bind($"{EntityNameReference} | Inside Enemy Spawn Curve", $"Inside enemy spawn curve for {EntityNameReference}.", Level.enemySpawnChanceThroughoutDay) : null,
            OutsideEnemySpawnCurve = GenerateEnemySpawnCurveConfigs && Level.spawnEnemiesAndScrap ? section.Bind($"{EntityNameReference} | Outside Enemy Spawn Curve", $"Outside enemy spawn curve for {EntityNameReference}.", Level.outsideEnemySpawnChanceThroughDay) : null,
            DaytimeEnemySpawnCurve = GenerateEnemySpawnCurveConfigs && Level.spawnEnemiesAndScrap ? section.Bind($"{EntityNameReference} | Daytime Enemy Spawn Curve", $"Daytime enemy spawn curve for {EntityNameReference}.", Level.daytimeEnemySpawnChanceThroughDay) : null,
            WeedEnemySpawnCurve = GenerateEnemySpawnCurveConfigs && Level.spawnEnemiesAndScrap ? section.Bind($"{EntityNameReference} | Weed Enemy Spawn Curve", $"Weed enemy spawn curve for {EntityNameReference}.", WeedEnemySpawnChanceThroughDay) : null,

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
            DuskBaseConfig.AssignValueIfNotNull(moonConfig.WeedEnemyPowerCount, MaxWeedEnemyPowerCount);

            DuskBaseConfig.AssignValueIfNotNull(moonConfig.InsideDiversityPowerCount, Level.maxInsideDiversityPowerCount);
            DuskBaseConfig.AssignValueIfNotNull(moonConfig.OutsideDiversityPowerCount, Level.maxOutsideDiversityPowerCount);
            DuskBaseConfig.AssignValueIfNotNull(moonConfig.DaytimeDiversityPowerCount, MaxDaytimeDiversityPowerCount);
            DuskBaseConfig.AssignValueIfNotNull(moonConfig.WeedDiversityPowerCount, MaxWeedDiversityPowerCount);

            DuskBaseConfig.AssignValueIfNotNull(moonConfig.InsideEnemySpawnRange, Level.spawnProbabilityRange);
            DuskBaseConfig.AssignValueIfNotNull(moonConfig.OutsideEnemySpawnRange, OutsideEnemiesSpawnProbabilityRange);
            DuskBaseConfig.AssignValueIfNotNull(moonConfig.DaytimeEnemySpawnRange, Level.daytimeEnemiesProbabilityRange);
            DuskBaseConfig.AssignValueIfNotNull(moonConfig.WeedEnemySpawnRange, WeedEnemiesProbabilityRange);

            DuskBaseConfig.AssignValueIfNotNull(moonConfig.InsideEnemySpawnCurve, Level.enemySpawnChanceThroughoutDay);
            DuskBaseConfig.AssignValueIfNotNull(moonConfig.OutsideEnemySpawnCurve, Level.outsideEnemySpawnChanceThroughDay);
            DuskBaseConfig.AssignValueIfNotNull(moonConfig.DaytimeEnemySpawnCurve, Level.daytimeEnemySpawnChanceThroughDay);
            DuskBaseConfig.AssignValueIfNotNull(moonConfig.WeedEnemySpawnCurve, WeedEnemySpawnChanceThroughDay);

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
    [field: SerializeField]
    public bool GenerateWeightsConfig { get; private set; } = true;

    private IWeightModifierSource<int> _spawnWeightSource = null!;
    private DawnWeightedValue<int> _weights;

    public MoonSceneConfig MoonSceneConfig { get; private set; }

    public DawnWeightedValue<int> Weight(ConfigContext section, int sceneCount)
    {
        if (sceneCount <= 1)
        {
            GenerateWeightsConfig = false;
        }

        MoonSceneConfig = CreateMoonSceneConfig(section);

        _spawnWeightSource = DuskContentDefinition.CreateSpawnWeightSource(
            () => DuskContentDefinition.GetConfigWeights(null, new List<NamespacedConfigWeight>()),
            () => DuskContentDefinition.GetConfigWeights(null, new List<NamespacedConfigWeight>()),
            () => DuskContentDefinition.GetConfigWeights(MoonSceneConfig.WeatherSpawnWeights, WeatherSpawnWeightsConfig),
            () => DuskContentDefinition.GetConfigWeights(null, new List<IntComparisonConfigWeight>()),
            () => MoonSceneConfig.BaseWeight?.Value ?? BaseWeight);

        WeightProfile<int> weightProfile = new(DawnWeightChannels.MoonSceneRarity.Policy);
        weightProfile.AddSource(_spawnWeightSource);
        _weights = new DawnWeightedValue<int>(DawnWeightChannels.MoonSceneRarity, weightProfile);
        return _weights;
    }

    public MoonSceneConfig CreateMoonSceneConfig(ConfigContext section)
    {
        MoonSceneConfig moonSceneConfig = new(section, SceneName)
        {
            BaseWeight = GenerateWeightsConfig ? section.Bind($"{SceneName} | Base Weight", $"Base Weight for Moon Scene: {SceneName}.", BaseWeight) : null,
            WeatherSpawnWeights = GenerateWeightsConfig ? section.Bind($"{SceneName} | Weather Spawn Weights", $"Weather Weights for Moon Scene: {SceneName}.", WeatherSpawnWeightsConfig.ConvertManyToString()) : null,
        };

        if (!moonSceneConfig.UserAllowedToEdit())
        {
            DuskBaseConfig.AssignValueIfNotNull(moonSceneConfig.BaseWeight, BaseWeight);
            DuskBaseConfig.AssignValueIfNotNull(moonSceneConfig.WeatherSpawnWeights, WeatherSpawnWeightsConfig.ConvertManyToString());
        }
        return moonSceneConfig;
    }
}