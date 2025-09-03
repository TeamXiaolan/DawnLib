using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace CodeRebirthLib.CRMod;

[Serializable]
public abstract class EntityData
{
    public abstract NamespacedKey Key { get; }
}

[Serializable]
public abstract class EntityData<T> : EntityData where T : CRMContentReference, new()
{
    [FormerlySerializedAs("reference")]
    [SerializeReference]
    [AssertFieldNotNull]
    T _reference = new T();

    public override NamespacedKey Key => _reference == null ? null : _reference.Key;

    public EntityData()
    {
        if (_reference == null)
        {
            _reference = new T();
        }
    }
}