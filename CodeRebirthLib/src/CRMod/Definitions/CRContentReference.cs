using System;

namespace CodeRebirthLib.CRMod;
[Serializable]
public abstract class CRContentReference
{
    public abstract Type Type { get; }
    public abstract NamespacedKey Key { get; protected set; }

    private string assetGUID;
}

[Serializable]
public abstract class CRContentReference<TDef, TInfo> : CRContentReference where TInfo : INamespaced<TInfo> where TDef : CRContentDefinition
{
    public CRContentReference()
    {
        Key = NamespacedKey<TInfo>.From("", "");
    }
    protected CRContentReference(NamespacedKey<TInfo> key)
    {
        Key = key;
    }

    public NamespacedKey<TInfo> TypedKey => Key.AsTyped<TInfo>();
    public override NamespacedKey Key { get; protected set; }
    public override Type Type => typeof(TInfo);

    public abstract bool TryResolve(out TInfo info);
    public abstract TInfo Resolve();
}