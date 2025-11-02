using UnityEngine;

namespace Dusk;

[AddComponentMenu($"{DuskModConstants.ProgressiveComponents}/Unlockable Upgrade Scrap")]
public class UnlockableUpgradeScrap : GrabbableObject
{
    [field: SerializeReference]
    public DuskUnlockableReference UnlockableReference { get; private set; } = null!;
}