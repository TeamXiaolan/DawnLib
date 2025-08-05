using UnityEngine;

namespace CodeRebirthLib.ContentManagement.Unlockables.Progressive;
public class UnlockableUpgradeScrap : GrabbableObject
{
    [field: SerializeField]
    public CRUnlockableReference CRUnlockableReference { get; private set; } = null!;
}