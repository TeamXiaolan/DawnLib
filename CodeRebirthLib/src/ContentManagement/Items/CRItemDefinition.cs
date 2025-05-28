using CodeRebirthLib.AssetManagement;
using UnityEngine;
using UnityEngine.Serialization;

namespace CodeRebirthLib.ContentManagement.Items;

[CreateAssetMenu(fileName = "New Item Definition", menuName = "CodeRebirthLib/Item Definition")]
public class CRItemDefinition : CRContentDefinition
{
    [field: FormerlySerializedAs("item"), SerializeField]
    public Item Item { get; private set; }
    
    [field: FormerlySerializedAs("terminalNode"), SerializeField]
    public TerminalNode? TerminalNode { get; private set; }
}