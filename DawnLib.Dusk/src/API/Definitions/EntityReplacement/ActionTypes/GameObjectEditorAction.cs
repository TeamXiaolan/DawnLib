using UnityEngine;

namespace Dusk;

[CreateAssetMenu(fileName = "New GameObject Editor Action", menuName = $"Entity Replacements/Actions/GameObject Editor Action")]
public class GameObjectEditorAction : Hierarchy
{
    [field: SerializeField]
    public Vector3 PositionOffset { get; private set; }
    [field: SerializeField]
    public Vector3 RotationOffset { get; private set; }

    public override void Apply(Transform rootTransform)
    {
        GameObject gameObject = !string.IsNullOrEmpty(HierarchyPath) ? rootTransform.Find(HierarchyPath).gameObject : rootTransform.gameObject;
        gameObject.transform.localPosition += PositionOffset;
        gameObject.transform.localRotation *= Quaternion.Euler(RotationOffset);
    }
}