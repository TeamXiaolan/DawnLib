using System;

namespace Dawn.Dusk;

[Serializable]
public class CRMItemReference : CRMContentReference<CRMItemDefinition, CRItemInfo>
{
    public CRMItemReference() : base()
    { }
    public CRMItemReference(NamespacedKey<CRItemInfo> key) : base(key)
    { }
    public override bool TryResolve(out CRItemInfo info)
    {
        return LethalContent.Items.TryGetValue(TypedKey, out info);
    }

    public override CRItemInfo Resolve()
    {
        return LethalContent.Items[TypedKey];
    }
}