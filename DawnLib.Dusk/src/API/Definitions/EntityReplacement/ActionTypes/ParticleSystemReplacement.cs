using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dusk;

[CreateAssetMenu(fileName = "New ParticleSystem Replacement", menuName = $"Entity Replacements/Actions/ParticleSystem Replacement")]
public class ParticleSystemReplacement : Hierarchy
{
    [field: SerializeField]
    public ParticleSystem NewParticleSystem { get; private set; }

    public override IEnumerator Apply(Transform rootTransform, bool immediate = false)
    {
        if (!immediate)
        {
            yield return null;
        }

        List<Transform> oldTransforms = GetComponentsWithHierarchyPaths<Transform>(rootTransform);
        foreach (Transform oldTransform in oldTransforms)
        {
            GameObject oldGameObject = oldTransform.gameObject;
            GameObject newGameObject = GameObject.Instantiate(NewParticleSystem.gameObject, oldGameObject.transform.parent);
            newGameObject.name = oldGameObject.name;
            Destroy(oldGameObject);
        }
    }
}