using UnityEngine;

namespace Dusk;
public class UnlockableUpgradeScrap : GrabbableObject
{
    [field: SerializeReference]
    public DuskUnlockableReference UnlockableReference { get; private set; } = null!;
}