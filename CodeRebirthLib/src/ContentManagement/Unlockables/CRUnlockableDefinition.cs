using CodeRebirthLib.AssetManagement;
using LethalLib.Extras;
using UnityEngine;
using UnityEngine.Serialization;

namespace CodeRebirthLib.ContentManagement.Unlockables;

[CreateAssetMenu(fileName = "New Unlockable Definition", menuName = "CodeRebirthLib/Unlockable Definition")]
public class CRUnlockableDefinition : CRContentDefinition
{
    [field: FormerlySerializedAs("unlockableItemDef"), SerializeField]
    public UnlockableItemDef UnlockableItemDef { get; private set; }

    [field: FormerlySerializedAs("DenyPurchaseNode"), SerializeField]
    public TerminalNode? DenyPurchaseNode { get; private set; }
}