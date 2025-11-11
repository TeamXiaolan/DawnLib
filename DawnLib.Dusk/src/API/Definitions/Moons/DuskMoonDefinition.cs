using System;
using System.Collections.Generic;
using Dawn;
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

    [field: Header("Configs")]
    [field: SerializeField]
    public int Cost { get; private set; }
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
        });
    }

    public MoonConfig CreateMoonConfig(ConfigContext context)
    {
        return new MoonConfig
        {
            DisableUnlockRequirements = GenerateDisableUnlockConfig && TerminalPredicate ? context.Bind($"{EntityNameReference} | Disable Unlock Requirements", $"Whether {EntityNameReference} should have it's unlock requirements disabled.", false) : null,
            DisablePricingStrategy = GenerateDisablePricingStrategyConfig && PricingStrategy ? context.Bind($"{EntityNameReference} | Disable Pricing Strategy", $"Whether {EntityNameReference} should have it's pricing strategy disabled.", false) : null,
            Cost = GenerateCostConfig ? context.Bind($"{EntityNameReference} | Cost", $"Cost for {EntityNameReference} in the shop.", Cost) : null,
        };
    }

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
    [field: SerializeField]
    public string WeatherWeights { get; private set; } = "None=*1, DustClouds=*1, Rainy=*1, Stormy=*1, Foggy=*1, Flooded=*1, Eclipsed=*1";

    public int Weight() // todo: this shouldn't return an int but an IProviderTable<int>??
    {
        SpawnWeightsPreset newSpawnWeightsPreset = new();
        newSpawnWeightsPreset.SetupSpawnWeightsPreset(string.Empty, string.Empty, WeatherWeights);
        return BaseWeight + newSpawnWeightsPreset.GetWeight();
    }
}