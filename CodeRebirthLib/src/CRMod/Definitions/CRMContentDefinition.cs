using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Configuration;
using CodeRebirthLib.Internal;
using UnityEngine;
using UnityEngine.Serialization;

namespace CodeRebirthLib.CRMod;

public abstract class CRMContentDefinition : ScriptableObject
{
    public abstract NamespacedKey Key { get; protected set; }

    [FormerlySerializedAs("ConfigEntries")]
    [SerializeField]
    private List<CRDynamicConfig> _configEntries;

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

    public string GetDefaultKey()
    {
        string normalizedName = NamespacedKey.NormalizeStringForNamespacedKey(EntityNameReference, false);
        return normalizedName;
    }

    protected abstract string EntityNameReference { get; }
}

public abstract class CRMContentDefinition<T, TInfo> : CRMContentDefinition where T : EntityData where TInfo : INamespaced<TInfo>
{
    public NamespacedKey<TInfo> TypedKey => Key.AsTyped<TInfo>();

    [field: SerializeField, InspectorName("Namespace")]
    public override NamespacedKey Key { get; protected set; }

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

                    return false; // throw?
                }));
        }
        catch (InvalidOperationException ex)
        {
            mod.Logger?.LogError($"{this} with {Key} failed to find a matching entity. {ex.Message}");
        }

        base.Register(mod);
    }

    public abstract void Register(CRMod mod, T data);

    public abstract List<T> GetEntities(CRMod mod);
}