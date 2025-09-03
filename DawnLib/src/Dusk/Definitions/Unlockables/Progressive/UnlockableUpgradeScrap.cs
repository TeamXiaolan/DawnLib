using UnityEngine;

namespace Dawn.Dusk;
public class UnlockableUpgradeScrap : GrabbableObject
{
    [field: SerializeField]
    public CRMUnlockableReference CRUnlockableReference { get; private set; } = null!;
}