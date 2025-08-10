using UnityEngine;

namespace CodeRebirthLib;
public class UnlockableUpgradeScrap : GrabbableObject
{
    [field: SerializeField]
    public CRUnlockableReference CRUnlockableReference { get; private set; } = null!;
}