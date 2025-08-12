using System;

namespace CodeRebirthLib.CRMod;
[Serializable]
public abstract class CRContentReference
{
    public abstract Type Type { get; }
    public abstract NamespacedKey Key { get; }
}

[Serializable]
public abstract class CRContentReference<TDef, TInfo> : CRContentReference where TInfo : INamespaced<TInfo> where TDef : CRContentDefinition
{
    public NamespacedKey<TInfo> TypedKey { get; private set; }
    public override NamespacedKey Key => TypedKey;
    public override Type Type => typeof(TInfo);

    public abstract bool TryResolve(out TInfo info);
}