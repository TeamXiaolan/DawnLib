using UnityEngine;

namespace CodeRebirthLib.CRMod;
public class UnlockableUpgradeScrap : GrabbableObject
{
    [field: SerializeField]
    public CRMUnlockableReference CRUnlockableReference { get; private set; } = null!;
}