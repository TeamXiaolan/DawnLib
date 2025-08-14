using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using BepInEx.Configuration;
using CodeRebirthLib.Internal;
using UnityEngine;
using UnityEngine.Serialization;

namespace CodeRebirthLib.CRMod;

public abstract class CRMContentDefinition : ScriptableObject
{
    public abstract NamespacedKey Key { get; }

    [FormerlySerializedAs("ConfigEntries")]
    [SerializeField]
    private List<CRDynamicConfig> _configEntries;

    protected abstract string EntityNameReference { get; }

    internal readonly Dictionary<string, ConfigEntryBase> generalConfigs = new();
    public CRMod Mod { get; private set; }

    internal AssetBundleData AssetBundleData;

    public virtual void Register(CRMod mod)
    {
        if (AssetBundleData == null)
        {
            mod.Logger?.LogError($"BUG! Tried to register {name} without setting AssetBundleData?");
            return;
        }

        Mod = mod;
        using ConfigContext context = mod.ConfigManager.CreateConfigSectionForBundleData(AssetBundleData);
        foreach (CRDynamicConfig configDefinition in _configEntries)
        {
            generalConfigs[ConfigManager.CleanStringForConfig(configDefinition.settingName)] = mod.ConfigManager.CreateDynamicConfig(configDefinition, context);
        }
    }
}

public abstract class CRMContentDefinition<T, TInfo> : CRMContentDefinition where T : EntityData where TInfo : INamespaced<TInfo>
{
    [field: SerializeField, InspectorName("Key")]
    public NamespacedKey<TInfo> TypedKey { get; private set; }

    public override NamespacedKey Key => TypedKey;

    public override void Register(CRMod mod)
    {
        try
        {
            Register(mod,
                GetEntities(mod).First(it =>
                {
                    if (it.Key != null)
                    {
                        Debuggers.CRMContentDefinition?.Log($"{this} | Comparing {Key} with {it.Key}.");
                        return Equals(it.Key, Key);
                    }
                    return it.entityName == EntityNameReference;

                }));
        }
        catch (InvalidOperationException ex)
        {
            mod.Logger?.LogError($"{this} with {EntityNameReference} failed to find a matching entity. {ex.Message}");
        }

        base.Register(mod);
    }

    public abstract void Register(CRMod mod, T data);

    public abstract List<T> GetEntities(CRMod mod);
}