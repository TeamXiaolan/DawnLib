using System.Collections.Generic;
using Dawn;
using Dusk.Weights;
using Unity.Netcode;
using UnityEngine;

namespace Dusk;

[CreateAssetMenu(fileName = "New Weather Definition", menuName = $"{DuskModConstants.Definitions}/Weather Definition")]
public class DuskWeatherDefinition : DuskContentDefinition<DawnWeatherEffectInfo>
{
    [field: SerializeField]
    public WeatherEffect WeatherEffect { get; private set; }

    [field: SerializeField]
    public float LerpSpeed { get; private set; } = 1f;

    [field: Space(10)]
    [field: Header("Configs | Main")]
    [field: SerializeField]
    public float ScrapValueMultiplier { get; private set; } = 1f;
    [field: SerializeField]
    public float ScrapAmountMultiplier { get; private set; } = 1f;

    [field: Header("Configs | Weights")]
    [field: SerializeField]
    public List<NamespacedConfigWeight> MoonSpawnWeightsConfig { get; private set; } = new();
    [field: SerializeField]
    public List<NamespacedConfigWeight> WeatherToWeatherSpawnWeightsConfig { get; private set; } = new();
    [field: SerializeField]
    public List<IntComparisonConfigWeight> RouteSpawnWeightsConfig { get; private set; } = new();
    [field: SerializeField]
    public bool GenerateSpawnWeightsConfig { get; private set; } = true;

    public SpawnWeightsPreset SpawnWeights { get; private set; } = new();
    public WeatherConfig Config { get; private set; }

    public override void Register(DuskMod mod)
    {
        base.Register(mod);
        if (WeatherEffect.effectObject != null)
        {
            WeatherEffect.effectObject.SetActive(false);
        }

        if (WeatherEffect.effectPermanentObject != null)
        {
            WeatherEffect.effectPermanentObject.SetActive(false);
        }

        using ConfigContext section = mod.ConfigManager.CreateConfigSectionForBundleData(AssetBundleData);
        Config = CreateWeatherConfig(section);
        BaseConfig = Config;

        List<UnresolvedNamespacedWeight> Moons = MoonSpawnWeightsConfig.ToUnresolvedWeights();
        if (Config.MoonSpawnWeights != null)
        {
            Moons = UnresolvedNamespacedWeight.ConvertManyFromString(Config.MoonSpawnWeights.Value);
        }

        List<UnresolvedNamespacedWeight> Weathers = WeatherToWeatherSpawnWeightsConfig.ToUnresolvedWeights();
        if (Config.WeatherToWeatherSpawnWeights != null)
        {
            Weathers = UnresolvedNamespacedWeight.ConvertManyFromString(Config.WeatherToWeatherSpawnWeights.Value);
        }

        List<IntComparisonConfigWeight> Routes = RouteSpawnWeightsConfig;
        if (Config.RouteSpawnWeights != null)
        {
            Routes = IntComparisonConfigWeight.ConvertManyFromString(Config.RouteSpawnWeights.Value);
        }

        SpawnWeights.SetupSpawnWeightsPreset(Moons, [], Weathers);
        SpawnWeights.AddRule(new RoutePriceRule(new RoutePriceWeightTransformer(Routes)));
        DawnLib.DefineWeatherEffect(TypedKey, WeatherEffect, builder =>
        {
            builder.OverrideLerpSpeed(LerpSpeed);
            builder.SetWeights(weightBuilder => weightBuilder.SetGlobalWeight(SpawnWeights));
            ApplyTagsTo(builder);
        });
    }

    public WeatherConfig CreateWeatherConfig(ConfigContext section)
    {
        WeatherConfig weatherConfig = new(section, EntityNameReference)
        {
            MoonSpawnWeights = GenerateSpawnWeightsConfig ? section.Bind($"{EntityNameReference} | Preset Moon Weights", $"Preset moon weights for {EntityNameReference}.", MoonSpawnWeightsConfig.ConvertManyToString()) : null,
            WeatherToWeatherSpawnWeights = GenerateSpawnWeightsConfig ? section.Bind($"{EntityNameReference} | Preset Weather Weights", $"Preset weather weights for {EntityNameReference}.", WeatherToWeatherSpawnWeightsConfig.ConvertManyToString()) : null,
            RouteSpawnWeights = GenerateSpawnWeightsConfig ? section.Bind($"{EntityNameReference} | Preset Route Weights", $"Preset route weights for {EntityNameReference}.", IntComparisonConfigWeight.ConvertManyToString(RouteSpawnWeightsConfig)) : null,

            ScrapValueMultiplier = section.Bind($"{EntityNameReference} | Scrap Value Multiplier", $"Amount that {EntityNameReference} multiplies the value of each Scrap spawned from a moon.", ScrapValueMultiplier),
            ScrapAmountMultiplier = section.Bind($"{EntityNameReference} | Scrap Amount Multiplier", $"Amount that {EntityNameReference} multiplies the number of Scraps spawned from a moon.", ScrapAmountMultiplier)
        };

        if (!weatherConfig.UserAllowedToEdit())
        {
            DuskBaseConfig.AssignValueIfNotNull(weatherConfig.MoonSpawnWeights, MoonSpawnWeightsConfig.ConvertManyToString());
            DuskBaseConfig.AssignValueIfNotNull(weatherConfig.WeatherToWeatherSpawnWeights, WeatherToWeatherSpawnWeightsConfig.ConvertManyToString());
            DuskBaseConfig.AssignValueIfNotNull(weatherConfig.RouteSpawnWeights, IntComparisonConfigWeight.ConvertManyToString(RouteSpawnWeightsConfig));

            DuskBaseConfig.AssignValueIfNotNull(weatherConfig.ScrapValueMultiplier, ScrapValueMultiplier);
            DuskBaseConfig.AssignValueIfNotNull(weatherConfig.ScrapAmountMultiplier, ScrapAmountMultiplier);
        }
        return weatherConfig;
    }

    public override void TryNetworkRegisterAssets()
    {
        if (WeatherEffect.effectObject && WeatherEffect.effectObject.TryGetComponent(out NetworkObject _))
        {
            DuskPlugin.Logger.LogWarning($"{WeatherEffect.name}'s EffectObject has a NetworkObject, meaning it likely uses a NetworkBehaviour, this is not supported through weathers, please reconsider your implementation.");
            DawnLib.RegisterNetworkPrefab(WeatherEffect.effectObject);
        }

        if (WeatherEffect.effectPermanentObject && WeatherEffect.effectPermanentObject.TryGetComponent(out NetworkObject _))
        {
            DuskPlugin.Logger.LogWarning($"{WeatherEffect.name}'s EffectPermanentObject has a NetworkObject, meaning it likely uses a NetworkBehaviour, this is not supported through weathers, please reconsider your implementation.");
            DawnLib.RegisterNetworkPrefab(WeatherEffect.effectPermanentObject);
        }
    }

    protected override string EntityNameReference => WeatherEffect.name;
}