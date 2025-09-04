using UnityEngine;

namespace Dusk;
public class ItemUpgradeScrap : GrabbableObject
{
    [field: SerializeReference]
    public DuskItemReference ItemReference { get; private set; } = null!;
}