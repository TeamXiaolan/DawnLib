using UnityEngine;

namespace Dusk;

[AddComponentMenu($"{DuskModConstants.ProgressiveComponents}/Item Upgrade Scrap")]
public class ItemUpgradeScrap : GrabbableObject
{
    [field: SerializeReference]
    public DuskItemReference ItemReference { get; private set; } = null!;
}