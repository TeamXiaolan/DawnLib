using System;
using UnityEngine;

namespace CodeRebirthLib.CRMod;

[Serializable]
public abstract class CRMContentReference
{
    public abstract Type Type { get; }
    public abstract Type DefinitionType { get; }
    public abstract NamespacedKey Key { get; protected set; }

    [field: SerializeField]
    private string assetGUID;
}

[Serializable]
public abstract class CRMContentReference<TDef, TInfo> : CRMContentReference where TInfo : INamespaced<TInfo> where TDef : CRMContentDefinition
{
    public CRMContentReference()
    {
        Key = NamespacedKey<TInfo>.From("", "");
    }

    protected CRMContentReference(NamespacedKey<TInfo> key)
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