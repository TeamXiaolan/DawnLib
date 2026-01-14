using System;
using Dawn;
using Unity.Netcode;
using UnityEngine;

namespace Dusk;

[Serializable]
public abstract class DuskContentReference : INetworkSerializable
{
    public abstract Type Type { get; }
    public abstract Type DefinitionType { get; }
    public abstract NamespacedKey Key { get; protected set; }

    [field: SerializeField]
    internal string assetGUID;
    public abstract void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter;
}

[Serializable]
public abstract class DuskContentReference<TDef, TInfo> : DuskContentReference where TInfo : INamespaced<TInfo> where TDef : DuskContentDefinition
{
    public DuskContentReference()
    {
        Key = NamespacedKey<TInfo>.From("", "");
    }

    protected DuskContentReference(NamespacedKey<TInfo> key)
    {
        Key = key;
    }

    public override void NetworkSerialize<T>(BufferSerializer<T> serializer)
    {
        NamespacedKey key = Key;
        serializer.SerializeNetworkSerializable(ref key);
        Key = key;
    }

    public NamespacedKey<TInfo> TypedKey => Key.AsTyped<TInfo>();
    [field: SerializeField]
    public override NamespacedKey Key { get; protected set; }
    public override Type Type => typeof(TInfo);
    public override Type DefinitionType => typeof(TDef);

    public abstract bool TryResolve(out TInfo info);
    public abstract TInfo Resolve();
}