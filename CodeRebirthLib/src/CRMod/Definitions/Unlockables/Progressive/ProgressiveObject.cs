using UnityEngine;

namespace CodeRebirthLib.CRMod;

[CreateAssetMenu(fileName = "New Progressive Object", menuName = "CodeRebirthLib/ProgressiveObject")]
public class ProgressiveObject : ScriptableObject
{
    [SerializeField]
    private TerminalNode _progressiveDenyNode;

    public TerminalNode ProgressiveDenyNode => _progressiveDenyNode ??= CreateDefaultProgressiveDenyNode();

    private static TerminalNode CreateDefaultProgressiveDenyNode()
    {
        TerminalNode node = CreateInstance<TerminalNode>();
        node.displayText = "Ship Upgrade or Decor is not unlocked";
        return node;
    }
}