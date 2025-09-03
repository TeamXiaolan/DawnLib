using UnityEngine;

namespace Dawn.Dusk;
public class ItemUpgradeScrap : GrabbableObject
{
    [field: SerializeField]
    public DuskItemReference ItemReference { get; private set; } = null!;
}