using System;

namespace CodeRebirthLib.CRMod;

[Serializable]
public class CRUnlockableReference : CRContentReference<CRItemDefinition, CRUnlockableItemInfo>
{
    public CRUnlockableReference() : base()
    { }
    public CRUnlockableReference(NamespacedKey<CRUnlockableItemInfo> key) : base(key)
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