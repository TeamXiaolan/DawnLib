using UnityEngine;

namespace Dawn.Dusk;
public class UnlockableUpgradeScrap : GrabbableObject
{
    [field: SerializeField]
    public DuskUnlockableReference UnlockableReference { get; private set; } = null!;
}