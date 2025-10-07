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
    public List<HierarchyReplacement> Replacements { get; private set; } = new();

    [field: Tooltip("This is where you'd add gameobjects, main use case is adding meshes or cosmetics to entities.")]
    [field: SerializeField]
    public List<GameObjectWithPath> GameObjectAddons { get; private set; } = new();

    public NamespacedKey<DuskEntityReplacementDefinition> TypedKey => _typedKey;
    public override NamespacedKey Key { get => TypedKey; protected set => _typedKey = value.AsTyped<DuskEntityReplacementDefinition>(); }

    // bongo todo: this is awful, and when migrating this stuff to be dawn info, this should probably be an interface or something
    internal bool IsDefault = false;

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
    public bool GenerateSpawnWeightsConfig { get; private set; }

    [field: Header("Configs | Misc")]
    [field: SerializeField]
    public bool GenerateDisableDateConfig { get; private set; }

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

        SpawnWeights.SetupSpawnWeightsPreset(MoonSpawnWeights, InteriorSpawnWeights, WeatherSpawnWeights);
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
            MoonSpawnWeights = GenerateSpawnWeightsConfig ? section.Bind($"{EntityNameReference} | Preset Moon Weights", $"Preset moon weights for {EntityNameReference}.", MoonSpawnWeights) : null,
            InteriorSpawnWeights = GenerateSpawnWeightsConfig ? section.Bind($"{EntityNameReference} | Preset Interior Weights", $"Preset interior weights for {EntityNameReference}.", InteriorSpawnWeights) : null,
            WeatherSpawnWeights = GenerateSpawnWeightsConfig ? section.Bind($"{EntityNameReference} | Preset Weather Weights", $"Preset weather weights for {EntityNameReference}.", WeatherSpawnWeights) : null,

            DisableDateCheck = GenerateDisableDateConfig && DatePredicate ? section.Bind($"{EntityNameReference} | Disable Date Check", $"Whether {EntityNameReference} should have it's date check disabled.", false) : null,
        };
    }

    protected override string EntityNameReference => TypedKey.Key;
}

public abstract class DuskEntityReplacementDefinition<TAI> : DuskEntityReplacementDefinition where TAI : class
{
    public abstract void Apply(TAI ai);
}