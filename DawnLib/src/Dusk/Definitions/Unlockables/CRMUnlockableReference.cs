using System;

namespace Dawn.Dusk;

[Serializable]
public class CRMUnlockableReference : CRMContentReference<CRMUnlockableDefinition, DawnUnlockableItemInfo>
{
    public CRMUnlockableReference() : base()
    { }
    public CRMUnlockableReference(NamespacedKey<DawnUnlockableItemInfo> key) : base(key)
    { }
    public override bool TryResolve(out DawnUnlockableItemInfo info)
    {
        return LethalContent.Unlockables.TryGetValue(TypedKey, out info);
    }

    public override DawnUnlockableItemInfo Resolve()
    {
        return LethalContent.Unlockables[TypedKey];
    }
}