using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Configuration;
using Dawn.Internal;
using UnityEngine;
using UnityEngine.Serialization;

namespace Dawn.Dusk;

public abstract class DuskContentDefinition : ScriptableObject
{
    public abstract NamespacedKey Key { get; protected set; }

    [FormerlySerializedAs("ConfigEntries")]
    [SerializeField]
    private List<DuskDynamicConfig> _configEntries = new();

    [SerializeField]
    internal List<NamespacedKey> _tags = new();

    internal readonly Dictionary<string, ConfigEntryBase> generalConfigs = new();
    public DuskMod Mod { get; private set; }

    internal AssetBundleData AssetBundleData;

    public virtual void Register(DuskMod mod)
    {
        if (AssetBundleData == null)
        {
            mod.Logger?.LogError($"BUG! Tried to register {name} without setting AssetBundleData?");
            return;
        }

        Mod = mod;
        using ConfigContext context = mod.ConfigManager.CreateConfigSectionForBundleData(AssetBundleData);
        foreach (DuskDynamicConfig configDefinition in _configEntries)
        {
            generalConfigs[ConfigManager.CleanStringForConfig(configDefinition.settingName)] = mod.ConfigManager.CreateDynamicConfig(configDefinition, context);
        }
    }

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
}

public abstract class DuskContentDefinition<T, TInfo> : DuskContentDefinition where T : EntityData where TInfo : INamespaced<TInfo>
{
    public NamespacedKey<TInfo> TypedKey => Key.AsTyped<TInfo>();

    [field: SerializeField, InspectorName("Namespace")]
    public override NamespacedKey Key { get; protected set; }

    public override void Register(DuskMod mod)
    {
        try
        {
            Register(mod,
                GetEntities(mod).First(it =>
                {
                    if (it.Key != null)
                    {
                        Debuggers.Dusk?.Log($"{this} | Comparing {Key} with {it.Key}.");
                        return Equals(it.Key, Key);
                    }

                    return false; // throw?
                }));
        }
        catch (InvalidOperationException ex)
        {
            mod.Logger?.LogError($"{this} with {Key} failed to find a matching entity. {ex.Message}");
        }

        base.Register(mod);
    }

    public abstract void Register(DuskMod mod, T data);

    public abstract List<T> GetEntities(DuskMod mod);
}