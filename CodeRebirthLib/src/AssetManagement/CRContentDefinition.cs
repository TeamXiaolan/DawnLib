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
    private CRMod _mod;

    internal AssetBundleData AssetBundleData;

    public virtual void Register(CRMod mod)
    {
        _mod = mod;
        foreach (CRDynamicConfig configDefinition in _configEntries)
        {
            _generalConfigs[ConfigManager.CleanStringForConfig(configDefinition.settingName)] = mod.ConfigManager.CreateDynamicConfig(configDefinition, AssetBundleData.configName);
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
            _mod.Logger?.LogWarning($"TryGetGeneralConfig: '{configName}' does not exist on '{name}', returning false and entry will be null");
        }

        entry = null;
        return false;
    }
}

public abstract class CRContentDefinition<T> : CRContentDefinition where T : EntityData
{
    public override void Register(CRMod mod)
    {
        base.Register(mod);
        Register(mod,
            GetEntities(mod).First(it =>
            {
                CodeRebirthLibPlugin.ExtendedLogging($"{this} | Comparing {EntityNameReference} with {it.entityName}.");
                return it.entityName == EntityNameReference;
            }));
    }

    public abstract void Register(CRMod mod, T data);

    public abstract List<T> GetEntities(CRMod mod);
}