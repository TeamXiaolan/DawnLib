using System;

namespace Dawn.Dusk;

[Serializable]
public class DuskItemReference : DuskContentReference<DuskItemDefinition, DawnItemInfo>
{
    public DuskItemReference() : base()
    { }
    public DuskItemReference(NamespacedKey<DawnItemInfo> key) : base(key)
    { }
    public override bool TryResolve(out DawnItemInfo info)
    {
        return LethalContent.Items.TryGetValue(TypedKey, out info);
    }

    public override DawnItemInfo Resolve()
    {
        return LethalContent.Items[TypedKey];
    }
}