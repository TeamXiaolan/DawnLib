using UnityEngine;

namespace CodeRebirthLib.CRMod;
public class UnlockableUpgradeScrap : GrabbableObject
{
    [field: SerializeField]
    public CRUnlockableReference CRUnlockableReference { get; private set; } = null!;
}