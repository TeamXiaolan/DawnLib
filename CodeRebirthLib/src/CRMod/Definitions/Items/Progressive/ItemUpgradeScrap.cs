using UnityEngine;

namespace CodeRebirthLib.CRMod;
public class ItemUpgradeScrap : GrabbableObject
{
    [field: SerializeField]
    public CRItemReference CRItemReference { get; private set; } = null!;
}