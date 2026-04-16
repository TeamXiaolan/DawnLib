using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dawn;
using Dawn.Internal;
using Dusk.Weights;
using Unity.Netcode;
using UnityEngine;

namespace Dusk;

public abstract class DuskEntityReplacementDefinition : DuskContentDefinition, INamespaced<DuskEntityReplacementDefinition>
{
    [field: SerializeField, InspectorName("Namespace"), DefaultKeySource("GetDefaultKey", false)]
    private NamespacedKey<DuskEntityReplacementDefinition> _typedKey;

    [field: SerializeField]
    public string SkinName { get; private set; }

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
    public List<IntComparisonConfigWeight> RouteSpawnWeightsConfig { get; private set; } = new();

    [field: SerializeField]
    public bool GenerateSpawnWeightsConfig { get; private set; } = true;

    [field: Header("Configs | Misc")]
    [field: SerializeField]
    public bool GenerateDisableDateConfig { get; private set; } = true;
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
    public ProviderTable<int?, DawnMoonInfo, SpawnWeightContext> Weights { get; private set; }
    public EntityReplacementConfig Config { get; private set; }

    public override void TryNetworkRegisterAssets()
    {
        foreach (GameObject gameObject in GameObjectAddons.Select(x => x.GameObjectToCreate))
        {
            if (!gameObject.TryGetComponent(out NetworkObject _))
                continue;

            DawnLib.RegisterNetworkPrefab(gameObject);
        }
    }

    internal void RegisterAsDefault()
    {
        IsDefault = true;
        Weights = new WeightTableBuilder<DawnMoonInfo, SpawnWeightContext>().SetGlobalWeight(100).Build();
    }

    public override void Register(DuskMod mod)
    {
        if (IsDefault)
        {
            RegisterAsDefault();
            return;
        }

        base.Register(mod);
        using ConfigContext section = mod.ConfigManager.CreateConfigSectionForBundleData(AssetBundleData);
        Config = CreateEntityReplacementConfig(section);
        BaseConfig = Config;

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

        Weights = new WeightTableBuilder<DawnMoonInfo, SpawnWeightContext>()
            .SetGlobalWeight(SpawnWeights)
            .Build();

        if (DatePredicate != null)
        {
            DatePredicate = DatePredicate.Instantiate(DatePredicate);
        }
        bool disableDateCheck = Config.DisableDateCheck?.Value ?? false;
        if (DatePredicate != null && !disableDateCheck)
        {
            DatePredicate.Register(Key);
        }
        DuskModContent.EntityReplacements.Register(this);
    }

    public EntityReplacementConfig CreateEntityReplacementConfig(ConfigContext section)
    {
        EntityReplacementConfig entityReplacementConfig = new(section, EntityNameReference)
        {
            MoonSpawnWeights = GenerateSpawnWeightsConfig ? section.Bind($"{EntityNameReference} | Preset Moon Weights", $"Preset moon weights for {EntityNameReference}.", MoonSpawnWeightsConfig.Count > 0 ? NamespacedConfigWeight.ConvertManyToString(MoonSpawnWeightsConfig) : MoonSpawnWeightsCompat) : null,
            InteriorSpawnWeights = GenerateSpawnWeightsConfig ? section.Bind($"{EntityNameReference} | Preset Interior Weights", $"Preset interior weights for {EntityNameReference}.", InteriorSpawnWeightsConfig.Count > 0 ? NamespacedConfigWeight.ConvertManyToString(InteriorSpawnWeightsConfig) : InteriorSpawnWeightsCompat) : null,
            WeatherSpawnWeights = GenerateSpawnWeightsConfig ? section.Bind($"{EntityNameReference} | Preset Weather Weights", $"Preset weather weights for {EntityNameReference}.", WeatherSpawnWeightsConfig.Count > 0 ? NamespacedConfigWeight.ConvertManyToString(WeatherSpawnWeightsConfig) : WeatherSpawnWeightsCompat) : null,

            DisableDateCheck = GenerateDisableDateConfig && DatePredicate ? section.Bind($"{EntityNameReference} | Disable Date Check", $"Whether {EntityNameReference} should have it's date check disabled.", false) : null
        };

        if (!entityReplacementConfig.UserAllowedToEdit())
        {
            DuskBaseConfig.AssignValueIfNotNull(entityReplacementConfig.MoonSpawnWeights, MoonSpawnWeightsConfig.Count > 0 ? NamespacedConfigWeight.ConvertManyToString(MoonSpawnWeightsConfig) : MoonSpawnWeightsCompat);
            DuskBaseConfig.AssignValueIfNotNull(entityReplacementConfig.InteriorSpawnWeights, InteriorSpawnWeightsConfig.Count > 0 ? NamespacedConfigWeight.ConvertManyToString(InteriorSpawnWeightsConfig) : InteriorSpawnWeightsCompat);
            DuskBaseConfig.AssignValueIfNotNull(entityReplacementConfig.WeatherSpawnWeights, WeatherSpawnWeightsConfig.Count > 0 ? NamespacedConfigWeight.ConvertManyToString(WeatherSpawnWeightsConfig) : WeatherSpawnWeightsCompat);
            DuskBaseConfig.AssignValueIfNotNull(entityReplacementConfig.RouteSpawnWeights, IntComparisonConfigWeight.ConvertManyToString(RouteSpawnWeightsConfig));

            DuskBaseConfig.AssignValueIfNotNull(entityReplacementConfig.DisableDateCheck, false);
        }

        return entityReplacementConfig;
    }

    protected override string EntityNameReference => SkinName;
}

public abstract class DuskEntityReplacementDefinition<TAI> : DuskEntityReplacementDefinition where TAI : class
{
    public abstract IEnumerator Apply(TAI ai, bool immediate = false);

    public IEnumerator ApplyReplacementAndAddons(Transform transform, bool immediate = false)
    {
        foreach (Hierarchy hierarchyReplacement in Replacements)
        {
            if (immediate)
            {
                StartOfRoundRefs.Instance.StartCoroutine(hierarchyReplacement.Apply(transform, immediate));
            }
            else
            {
                yield return StartOfRoundRefs.Instance.StartCoroutine(hierarchyReplacement.Apply(transform, immediate));
            }
        }

        foreach (GameObjectWithPath gameObjectAddon in GameObjectAddons)
        {
            GameObject gameObject = !string.IsNullOrWhiteSpace(gameObjectAddon.PathToGameObject) ? transform.Find(gameObjectAddon.PathToGameObject).gameObject : transform.gameObject;
            if (gameObjectAddon.GameObjectToCreate.TryGetComponent(out NetworkObject networkObject) && !NetworkManager.Singleton.IsServer)
                continue;

            GameObject addOn = GameObject.Instantiate(gameObjectAddon.GameObjectToCreate, gameObject.transform);
            addOn.transform.SetLocalPositionAndRotation(gameObjectAddon.PositionOffset, Quaternion.Euler(gameObjectAddon.RotationOffset));
            if (networkObject == null)
                continue;

            networkObject.AutoObjectParentSync = false;
            networkObject.Spawn();
        }
    }
}