using UnityEngine;

namespace Dusk;

[CreateAssetMenu(fileName = "New Disable GameObject Replacement", menuName = $"Entity Replacements/Replacements/Disable GameObject Replacement")]
public class DisableGameObjectReplacement : HierarchyReplacement
{
    public override void Apply(Transform rootTransform)
    {
        GameObject gameObject = !string.IsNullOrEmpty(HierarchyPath) ? rootTransform.Find(HierarchyPath).gameObject : rootTransform.gameObject;
        gameObject.SetActive(false);
    }
}