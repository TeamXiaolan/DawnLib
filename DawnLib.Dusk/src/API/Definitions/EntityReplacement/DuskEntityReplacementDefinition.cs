using System.Collections.Generic;
using Dawn;
using Dawn.Preloader.Interfaces;
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
    public List<RendererReplacement> RendererReplacements { get; private set; } = new();

    [field: SerializeField]
    public List<GameObjectWithPath> GameObjectAddons { get; private set; } = new(); // TODO if the gameobject has a networkobject, i need to do the finnicky network object parenting stuff? or just disable auto object parent sync

    public NamespacedKey<DuskEntityReplacementDefinition> TypedKey => _typedKey;
    public override NamespacedKey Key { get => TypedKey; protected set => _typedKey = value.AsTyped<DuskEntityReplacementDefinition>(); }

    // bongo todo: this is awful, and when migrating this stuff to be dawn info, this should probably be an interface or something
    internal bool IsVanilla;
    
    [field: Space(10)]
    [field: Header("Configs | Spawn Weights")]
    [field: SerializeField]
    public string MoonSpawnWeights { get; private set; }
    [field: SerializeField]
    public string InteriorSpawnWeights { get; private set; }
    [field: SerializeField]
    public string WeatherSpawnWeights { get; private set; }
    [field: SerializeField]
    public bool GenerateSpawnWeightsConfig { get; private set; }

    public SpawnWeightsPreset SpawnWeights { get; private set; } = new();
    public ProviderTable<int?, DawnMoonInfo> Weights { get; private set; }
    public EntityReplacementConfig Config { get; private set; }

    public override void Register(DuskMod mod)
    {
        if (IsVanilla)
        {
            Weights = new WeightTableBuilder<DawnMoonInfo>().SetGlobalWeight(100).Build();
            return;
        }

        base.Register(mod);
        using ConfigContext section = mod.ConfigManager.CreateConfigSectionForBundleData(AssetBundleData);
        Config = CreateEntityReplacementConfig(section);

        SpawnWeightsPreset preset = new();
        preset.SetupSpawnWeightsPreset(MoonSpawnWeights, InteriorSpawnWeights, WeatherSpawnWeights);
        Weights = new WeightTableBuilder<DawnMoonInfo>()
            .SetGlobalWeight(preset)
            .Build();

        DuskModContent.EntityReplacements.Register(this);
    }

    public EntityReplacementConfig CreateEntityReplacementConfig(ConfigContext section)
    {
        return new EntityReplacementConfig
        {
            MoonSpawnWeights = GenerateSpawnWeightsConfig ? section.Bind($"{EntityNameReference} | Preset Moon Weights", $"Preset moon weights for {EntityNameReference}.", MoonSpawnWeights) : null,
            InteriorSpawnWeights = GenerateSpawnWeightsConfig ? section.Bind($"{EntityNameReference} | Preset Interior Weights", $"Preset interior weights for {EntityNameReference}.", InteriorSpawnWeights) : null,
            WeatherSpawnWeights = GenerateSpawnWeightsConfig ? section.Bind($"{EntityNameReference} | Preset Weather Weights", $"Preset weather weights for {EntityNameReference}.", WeatherSpawnWeights) : null,
        };
    }

    protected override string EntityNameReference => TypedKey.Key;
}

public abstract class DuskEntityReplacementDefinition<TAI> : DuskEntityReplacementDefinition where TAI : class
{
    public abstract void Apply(TAI ai);
}