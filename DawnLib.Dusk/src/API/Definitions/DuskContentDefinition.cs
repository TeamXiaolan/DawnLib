using System;
using System.Collections.Generic;
using BepInEx.Configuration;
using Dawn;
using Dawn.Utils;
using Dusk.Weights;
using UnityEngine;
using UnityEngine.Serialization;

namespace Dusk;

public abstract class DuskContentDefinition : ScriptableObject
{
    public abstract NamespacedKey Key { get; protected set; }

    [FormerlySerializedAs("ConfigEntries")]
    [SerializeField]
    private List<DuskDynamicConfig> _configEntries = new();

    [SerializeField, UnlockedNamespacedKey]
    internal List<NamespacedKey> _tags = new();

    public DuskBaseConfig? BaseConfig { get; set; }

    public virtual void Register(DuskRegistrationContext registrationContext)
    {
        TryNetworkRegisterAssets();
    }

    public virtual void RegisterPost(DuskRegistrationContext registrationContext)
    {
        using ConfigContext context = registrationContext.Mod.ConfigManager.CreateConfigSectionForBundleData(registrationContext.AssetBundleData);
        foreach (DuskDynamicConfig configDefinition in _configEntries)
        {
            ConfigEntryBase entry = registrationContext.Mod.ConfigManager.CreateDynamicConfig(BaseConfig?.UserAllowedToEdit() ?? true, configDefinition, context);
            registrationContext.RegisterConfig(configDefinition.settingName.CleanStringForConfig(), entry);
        }

        if (BaseConfig != null)
        {
            foreach (ConfigEntryBase entry in BaseConfig.ConfigEntries())
            {
                registrationContext.RegisterConfig(entry);
            }
        }
    }

    public abstract void TryNetworkRegisterAssets();

    public string GetDefaultKey()
    {
        string normalizedName = NamespacedKey.NormalizeStringForNamespacedKey(EntityNameReference, false);
        return normalizedName;
    }

    protected abstract string EntityNameReference { get; }
    protected void ApplyTagsTo(BaseInfoBuilder builder)
    {
        builder.SoloAddTags(_tags);
    }

    public static IWeightModifierSource<int> CreateSpawnWeightSource(Func<IEnumerable<UnresolvedNamespacedWeight>> getMoons, Func<IEnumerable<UnresolvedNamespacedWeight>> getInteriors, Func<IEnumerable<UnresolvedNamespacedWeight>> getWeathers, Func<IEnumerable<IntComparisonConfigWeight>> getRoutes, Func<int> getDefaultWeight)
    {
        return new CompositeIntWeightSource()
            .Add(new MoonIntWeightSource(getMoons))
            .Add(new MoonSceneIntWeightSource(getMoons))
            .Add(new DungeonIntWeightSource(getInteriors))
            .Add(new WeatherIntWeightSource(getWeathers))
            .Add(new RoutePriceIntWeightSource(getRoutes))
            .Add(new GlobalBaseIntSource(getDefaultWeight));
    }

    public static IEnumerable<UnresolvedNamespacedWeight> GetConfigWeights(ConfigEntry<string>? spawnWeightsConfig, List<NamespacedConfigWeight> spawnWeightDefaults)
    {
        if (spawnWeightsConfig != null)
        {
            return UnresolvedNamespacedWeight.ConvertManyFromString(spawnWeightsConfig.Value);
        }

        return spawnWeightDefaults.ToUnresolvedWeights();
    }

    public static IEnumerable<IntComparisonConfigWeight> GetConfigWeights(ConfigEntry<string>? spawnWeightsConfig, List<IntComparisonConfigWeight> spawnWeightDefaults)
    {
        if (spawnWeightsConfig != null)
        {
            return IntComparisonConfigWeight.ConvertManyFromString(spawnWeightsConfig.Value);
        }

        return spawnWeightDefaults;
    }

    public static string CurvesToConfigString(IEnumerable<NamespacedKeyWithAnimationCurve> curves)
    {
        List<string> parts = new();
        foreach (NamespacedKeyWithAnimationCurve curve in curves)
        {
            parts.Add($"{curve.Key} - {ConfigManager.ParseString(curve.Curve)}");
        }

        return string.Join(" | ", parts);
    }
}

public abstract class DuskContentDefinition<TInfo> : DuskContentDefinition where TInfo : INamespaced<TInfo>
{
    public NamespacedKey<TInfo> TypedKey => Key.AsTyped<TInfo>();

    [field: SerializeField, InspectorName("Namespace"), DefaultKeySource("GetDefaultKey", false)]
    public override NamespacedKey Key { get; protected set; }
}