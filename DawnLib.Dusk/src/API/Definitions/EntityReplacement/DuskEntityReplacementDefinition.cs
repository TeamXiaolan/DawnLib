using System;
using System.Collections;
using System.Collections.Generic;
using Dawn;
using Dusk.Weights;
using UnityEngine;

namespace Dusk;

public abstract class DuskEntityReplacementDefinition : DuskContentDefinition, INamespaced<DuskEntityReplacementDefinition>
{
    [field: SerializeField, InspectorName("Namespace"), UnlockedNamespacedKey]
    private NamespacedKey<DuskEntityReplacementDefinition> _typedKey;

    [field: SerializeField, InspectorName("Entity to be Replaced"), UnlockedNamespacedKey, Space(5)]
    public NamespacedKey EntityToReplaceKey { get; private set; }

    [field: SerializeField]
    public DatePredicate? DatePredicate { get; private set; }

    [field: Space(10)]
    [field: SerializeField]
    public List<Hierarchy> Replacements { get; private set; } = new();

    [field: Tooltip("This is where you'd add gameobjects, main use case is adding meshes or cosmetics to entities.")]
    [field: SerializeField]
    public List<GameObjectWithPath> GameObjectAddons { get; private set; } = new();

    public NamespacedKey<DuskEntityReplacementDefinition> TypedKey => _typedKey;
    public override NamespacedKey Key { get => TypedKey; protected set => _typedKey = value.AsTyped<DuskEntityReplacementDefinition>(); }

    // bongo todo: this is awful, and when migrating this stuff to be dawn info, this should probably be an interface or something
    internal bool IsDefault = false;

    [field: Header("Configs | Spawn Weights | Format: <Namespace>:<Key>=<Operation><Value>, i.e. magic_wesleys_mod:trite=+20")]
    [field: SerializeField]
    public List<NamespacedConfigWeight> MoonSpawnWeightsConfig { get; private set; } = new();
    [field: SerializeField]
    public List<NamespacedConfigWeight> InteriorSpawnWeightsConfig { get; private set; } = new();
    [field: SerializeField]
    public List<NamespacedConfigWeight> WeatherSpawnWeightsConfig { get; private set; } = new();

    [field: SerializeField]
    public bool GenerateSpawnWeightsConfig { get; private set; } = true;

    [field: Header("Configs | Misc")]
    [field: SerializeField]
    public bool GenerateDisableDateConfig { get; private set; } = true;
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
    public ProviderTable<int?, DawnMoonInfo> Weights { get; private set; }
    public EntityReplacementConfig Config { get; private set; }

    public override void Register(DuskMod mod)
    {
        if (IsDefault)
        {
            Weights = new WeightTableBuilder<DawnMoonInfo>().SetGlobalWeight(100).Build();
            return;
        }

        base.Register(mod);
        using ConfigContext section = mod.ConfigManager.CreateConfigSectionForBundleData(AssetBundleData);
        Config = CreateEntityReplacementConfig(section);

        List<NamespacedConfigWeight> Moons = NamespacedConfigWeight.ConvertManyFromString(Config.MoonSpawnWeights?.Value ?? MoonSpawnWeights);
        List<NamespacedConfigWeight> Interiors = NamespacedConfigWeight.ConvertManyFromString(Config.InteriorSpawnWeights?.Value ?? InteriorSpawnWeights);
        List<NamespacedConfigWeight> Weathers = NamespacedConfigWeight.ConvertManyFromString(Config.WeatherSpawnWeights?.Value ?? WeatherSpawnWeights);

        SpawnWeights.SetupSpawnWeightsPreset(Moons.Count > 0 ? Moons : MoonSpawnWeightsConfig, Interiors.Count > 0 ? Interiors : InteriorSpawnWeightsConfig, Weathers.Count > 0 ? Weathers : WeatherSpawnWeightsConfig);

        Weights = new WeightTableBuilder<DawnMoonInfo>()
            .SetGlobalWeight(SpawnWeights)
            .Build();

        bool disableDateCheck = Config.DisableDateCheck?.Value ?? false;
        if (DatePredicate && !disableDateCheck)
        {
            DatePredicate.Register(Key);
        }
        DuskModContent.EntityReplacements.Register(this);
    }

    public EntityReplacementConfig CreateEntityReplacementConfig(ConfigContext section)
    {
        return new EntityReplacementConfig
        {
            MoonSpawnWeights = GenerateSpawnWeightsConfig ? section.Bind($"{EntityNameReference} | Preset Moon Weights", $"Preset moon weights for {EntityNameReference}.", MoonSpawnWeightsConfig.Count > 0 ? NamespacedConfigWeight.ConvertManyToString(MoonSpawnWeightsConfig) : MoonSpawnWeights) : null,
            InteriorSpawnWeights = GenerateSpawnWeightsConfig ? section.Bind($"{EntityNameReference} | Preset Interior Weights", $"Preset interior weights for {EntityNameReference}.", InteriorSpawnWeightsConfig.Count > 0 ? NamespacedConfigWeight.ConvertManyToString(InteriorSpawnWeightsConfig) : MoonSpawnWeights) : null,
            WeatherSpawnWeights = GenerateSpawnWeightsConfig ? section.Bind($"{EntityNameReference} | Preset Weather Weights", $"Preset weather weights for {EntityNameReference}.", WeatherSpawnWeightsConfig.Count > 0 ? NamespacedConfigWeight.ConvertManyToString(WeatherSpawnWeightsConfig) : WeatherSpawnWeights) : null,

            DisableDateCheck = GenerateDisableDateConfig && DatePredicate ? section.Bind($"{EntityNameReference} | Disable Date Check", $"Whether {EntityNameReference} should have it's date check disabled.", false) : null,
        };
    }

    protected override string EntityNameReference => TypedKey.Key;
}

public abstract class DuskEntityReplacementDefinition<TAI> : DuskEntityReplacementDefinition where TAI : class
{
    public abstract IEnumerator Apply(TAI ai);
}