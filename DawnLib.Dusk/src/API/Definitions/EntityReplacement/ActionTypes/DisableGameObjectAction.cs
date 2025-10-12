using UnityEngine;

namespace Dusk;

[CreateAssetMenu(fileName = "New Disable GameObject Action", menuName = $"Entity Replacements/Actions/Disable GameObject Action")]
public class DisableGameObjectAction : Hierarchy
{
    public override void Apply(Transform rootTransform)
    {
        GameObject gameObject = !string.IsNullOrEmpty(HierarchyPath) ? rootTransform.Find(HierarchyPath).gameObject : rootTransform.gameObject;
        gameObject.SetActive(false);
    }
}