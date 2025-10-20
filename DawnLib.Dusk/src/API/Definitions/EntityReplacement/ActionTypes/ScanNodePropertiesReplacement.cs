using System.Collections;
using UnityEngine;


namespace Dusk;

[CreateAssetMenu(fileName = "New ScanNodeProperties Replacement", menuName = $"Entity Replacements/Actions/ScanNodeProperties Replacement")]
public class ScanNodePropertiesReplacement : Hierarchy
{
    [field: Tooltip("Leave empty if not replacing")]
    [field: SerializeField]
    public int MaxRange { get; private set; } = -1;
    [field: Tooltip("Leave at -1 if not replacing")]
    [field: SerializeField]
    public int MinRange { get; private set; } = -1;
    [field: SerializeField]
    public bool RequiresLineOfSight { get; private set; } = true;

    [field: Space(5f)]
    [field: SerializeField]
    public string HeaderText { get; private set; } = "Leave as such if not replacing";
    [field: SerializeField]
    public string SubText { get; private set; } = "Leave as such if not replacing";

    [field: Space(3f)]
    [field: Tooltip("0 = Blue | 1 = Red | 2 = Green")]
    [field: SerializeField]
    public int NodeType { get; private set; }

    public override IEnumerator Apply(Transform rootTransform)
    {
        yield return null;
        ScanNodeProperties scanNodeProperties = !string.IsNullOrEmpty(HierarchyPath) ? rootTransform.Find(HierarchyPath).GetComponent<ScanNodeProperties>() : rootTransform.GetComponent<ScanNodeProperties>();
        if (MaxRange > -1) scanNodeProperties.maxRange = MaxRange;
        if (MinRange > -1) scanNodeProperties.minRange = MinRange;
        scanNodeProperties.nodeType = NodeType;
        scanNodeProperties.requiresLineOfSight = RequiresLineOfSight;
        if (HeaderText != "Leave as such if not replacing") scanNodeProperties.headerText = HeaderText;
        if (SubText != "Leave as such if not replacing") scanNodeProperties.subText = SubText;
    }
}