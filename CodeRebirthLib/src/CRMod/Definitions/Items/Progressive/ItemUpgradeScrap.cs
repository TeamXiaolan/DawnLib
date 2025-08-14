using UnityEngine;

namespace CodeRebirthLib.CRMod;
public class ItemUpgradeScrap : GrabbableObject
{
    [field: SerializeField]
    public CRMItemReference CRItemReference { get; private set; } = null!;
}