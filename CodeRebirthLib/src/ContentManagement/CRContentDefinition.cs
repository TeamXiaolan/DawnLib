using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using BepInEx.Configuration;
using CodeRebirthLib.AssetManagement;
using CodeRebirthLib.ConfigManagement;
using UnityEngine;
using UnityEngine.Serialization;

namespace CodeRebirthLib.ContentManagement;

public abstract class CRContentDefinition : ScriptableObject
{
    [FormerlySerializedAs("ConfigEntries")] [SerializeField]
    private List<CRDynamicConfig> _configEntries;

    protected abstract string EntityNameReference { get; }

    private readonly Dictionary<string, ConfigEntryBase> _generalConfigs = new();
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
            _generalConfigs[ConfigManager.CleanStringForConfig(configDefinition.settingName)] = mod.ConfigManager.CreateDynamicConfig(configDefinition, context);
        }
    }

    public ConfigEntry<T> GetGeneralConfig<T>(string configName)
    {
        return (ConfigEntry<T>)_generalConfigs[configName];
    }

    public bool TryGetGeneralConfig<T>(string configName, [NotNullWhen(true)] out ConfigEntry<T>? entry)
    {
        if (_generalConfigs.TryGetValue(configName, out ConfigEntryBase configBase))
        {
            entry = (ConfigEntry<T>)configBase;
            return true;
        }

        if (CodeRebirthLibConfig.ExtendedLogging)
        {
            Mod.Logger?.LogWarning($"TryGetGeneralConfig: '{configName}' does not exist on '{name}', returning false and entry will be null");
        }

        entry = null;
        return false;
    }
}

public abstract class CRContentDefinition<T> : CRContentDefinition where T : EntityData
{
    public override void Register(CRMod mod)
    {
        try
        {
            Register(mod,
                GetEntities(mod).First(it =>
                {
                    if (!string.IsNullOrEmpty(it.EntityName))
                    {
                        CodeRebirthLibPlugin.ExtendedLogging($"{this} | Comparing {EntityNameReference} with {it.EntityName}.");
                        return it.EntityName == EntityNameReference;
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