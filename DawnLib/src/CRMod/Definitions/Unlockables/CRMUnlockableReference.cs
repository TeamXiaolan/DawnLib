using System;

namespace CodeRebirthLib.CRMod;

[Serializable]
public class CRMUnlockableReference : CRMContentReference<CRMUnlockableDefinition, CRUnlockableItemInfo>
{
    public CRMUnlockableReference() : base()
    { }
    public CRMUnlockableReference(NamespacedKey<CRUnlockableItemInfo> key) : base(key)
    { }
    public override bool TryResolve(out CRUnlockableItemInfo info)
    {
        return LethalContent.Unlockables.TryGetValue(TypedKey, out info);
    }

    public override CRUnlockableItemInfo Resolve()
    {
        return LethalContent.Unlockables[TypedKey];
    }
}