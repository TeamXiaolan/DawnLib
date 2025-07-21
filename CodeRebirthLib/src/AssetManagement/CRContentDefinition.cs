using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using BepInEx.Configuration;
using CodeRebirthLib.ConfigManagement;
using CodeRebirthLib.ContentManagement;
using UnityEngine;
using UnityEngine.Serialization;

namespace CodeRebirthLib.AssetManagement;

public abstract class CRContentDefinition : ScriptableObject
{
    [FormerlySerializedAs("ConfigEntries")] [SerializeField]
    private List<CRDynamicConfig> _configEntries;

    [field: SerializeField]
    public string EntityNameReference { get; private set; }

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
        Register(mod,
            GetEntities(mod).First(it =>
            {
                CodeRebirthLibPlugin.ExtendedLogging($"{this} | Comparing {EntityNameReference} with {it.entityName}.");
                return it.entityName == EntityNameReference;
            }));
        base.Register(mod);
    }

    public abstract void Register(CRMod mod, T data);

    public abstract List<T> GetEntities(CRMod mod);
}