using System;
using System.Collections.Generic;
using System.Linq;
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

    public virtual void Register(CRMod mod)
    {
        foreach (CRDynamicConfig configDefinition in ConfigEntries)
        {
            // todo
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