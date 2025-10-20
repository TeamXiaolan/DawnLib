using System.Collections;
using UnityEngine;

namespace Dusk;

[CreateAssetMenu(fileName = "New ParticleSystem Replacement", menuName = $"Entity Replacements/Actions/ParticleSystem Replacement")]
public class ParticleSystemReplacement : Hierarchy
{
    [field: SerializeField]
    public ParticleSystem NewParticleSystem { get; private set; }

    public override IEnumerator Apply(Transform rootTransform)
    {
        yield return null;
        GameObject oldGameObject = !string.IsNullOrEmpty(HierarchyPath) ? rootTransform.Find(HierarchyPath).gameObject : rootTransform.gameObject;
        GameObject newGameObject = GameObject.Instantiate(NewParticleSystem.gameObject, oldGameObject.transform.parent);
        newGameObject.name = oldGameObject.name;
        Destroy(oldGameObject);
    }
}