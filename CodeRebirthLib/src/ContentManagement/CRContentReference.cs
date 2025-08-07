using System;
using Unity.Netcode;
using UnityEngine;

namespace CodeRebirthLib.ContentManagement;

[Serializable]
public abstract class CRContentReference(string name) : INetworkSerializable
{
    [SerializeField]
    internal string entityName = name;

    [SerializeField]
    internal string assetGUID = string.Empty;

    public abstract Type ContentType { get; }
    abstract internal string GetEntityName(CRContentDefinition obj);

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref entityName);
    }

    public static implicit operator string?(CRContentReference reference)
    {
        return reference.entityName;
    }
}

[Serializable]
public abstract class CRContentReference<T>(string name) : CRContentReference(name) where T : CRContentDefinition
{
    public override Type ContentType => typeof(T);

    override internal string GetEntityName(CRContentDefinition obj)
    {
        return GetEntityName((T)obj);
    }
    protected abstract string GetEntityName(T obj);
}