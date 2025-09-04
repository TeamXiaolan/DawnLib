using System;
using Dawn;

namespace Dusk;

[Serializable]
public class DuskUnlockableReference : DuskContentReference<DuskUnlockableDefinition, DawnUnlockableItemInfo>
{
    public DuskUnlockableReference() : base()
    { }
    public DuskUnlockableReference(NamespacedKey<DawnUnlockableItemInfo> key) : base(key)
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