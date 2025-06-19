using LethalLib.Extras;
using UnityEngine;
using UnityEngine.Serialization;

namespace CodeRebirthLib.ContentManagement.Unlockables.Progressive;
public class UnlockableUpgradeScrap : GrabbableObject
{
    [field: SerializeField, FormerlySerializedAs("unlockableItemDef")]
    public UnlockableItemDef UnlockableItemDef { get; private set; } = null!;
}