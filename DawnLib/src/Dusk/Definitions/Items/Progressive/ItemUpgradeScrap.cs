using UnityEngine;

namespace Dawn.Dusk;
public class ItemUpgradeScrap : GrabbableObject
{
    [field: SerializeField]
    public CRMItemReference CRItemReference { get; private set; } = null!;
}