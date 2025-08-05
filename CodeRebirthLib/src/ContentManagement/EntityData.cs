using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace CodeRebirthLib.ContentManagement;

[Serializable]
public abstract class EntityData
{
    public abstract string EntityName { get; }

    [Obsolete("Use EntityName instead")]
    [HideInInspector]
    [SerializeField]
    public string entityName;
}

[Serializable]
public abstract class EntityData<T> : EntityData where T : CRContentReference
{
    [FormerlySerializedAs("reference")]
    [SerializeReference]
    T _reference;
    public override string EntityName => _reference.entityName;

    public EntityData()
    {
        if (_reference == null)
        {
            _reference = (T)typeof(T).GetConstructor([typeof(string)]).Invoke([""]);
        }
    }
} 