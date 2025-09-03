using System;
using UnityEngine;

namespace Dawn.Dusk;

[Serializable]
public abstract class DuskContentReference
{
    public abstract Type Type { get; }
    public abstract Type DefinitionType { get; }
    public abstract NamespacedKey Key { get; protected set; }

    [field: SerializeField]
    internal string assetGUID;
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

    public NamespacedKey<TInfo> TypedKey => Key.AsTyped<TInfo>();
    [field: SerializeField]
    public override NamespacedKey Key { get; protected set; }
    public override Type Type => typeof(TInfo);
    public override Type DefinitionType => typeof(TDef);

    public abstract bool TryResolve(out TInfo info);
    public abstract TInfo Resolve();
}