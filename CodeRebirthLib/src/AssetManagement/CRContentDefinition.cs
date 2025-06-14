using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Configuration;
using CodeRebirthLib.ConfigManagement;
using CodeRebirthLib.ContentManagement;
using UnityEngine;
using UnityEngine.Serialization;

namespace CodeRebirthLib.AssetManagement;
public abstract class CRContentDefinition : ScriptableObject
{
    [FormerlySerializedAs("ConfigEntries")]
    public List<CRDynamicConfig> _configEntries;

    public IReadOnlyList<CRDynamicConfig> ConfigEntries => _configEntries;
    
    [field: SerializeField]
    public string EntityNameReference { get; private set; }

    internal AssetBundleData AssetBundleData;

    public Dictionary<string, ConfigEntryBase> GeneralConfigs { get; private set; } = new();
    
    public virtual void Register(CRMod mod)
    {
        foreach (CRDynamicConfig configDefinition in ConfigEntries)
        {
            GeneralConfigs[ConfigManager.CleanStringForConfig(configDefinition.settingName)] = mod.ConfigManager.CreateDynamicConfig(configDefinition, AssetBundleData.configName);
        }
    }
}

public abstract class CRContentDefinition<T> : CRContentDefinition where T : EntityData
{
    public override void Register(CRMod mod)
    {
        base.Register(mod);
        Register(mod, GetEntities(mod).First(it => it.entityName == EntityNameReference));
    }

    public abstract void Register(CRMod mod, T data);

    public abstract List<T> GetEntities(CRMod mod);
}