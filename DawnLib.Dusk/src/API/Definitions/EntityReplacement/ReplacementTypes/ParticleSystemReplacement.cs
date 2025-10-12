using System;
using UnityEngine;

namespace Dusk;

[CreateAssetMenu(fileName = "New ParticleSystem Replacement", menuName = $"Entity Replacements/Replacements/ParticleSystem Replacement")]
public class ParticleSystemReplacement : HierarchyReplacement
{
    [field: SerializeField]
    public ParticleSystem NewParticleSystem { get; private set; }

    public override void Apply(Transform rootTransform)
    {
        GameObject oldGameObject = !string.IsNullOrEmpty(HierarchyPath) ? rootTransform.Find(HierarchyPath).gameObject : rootTransform.gameObject;
        GameObject newGameObject = GameObject.Instantiate(NewParticleSystem.gameObject, oldGameObject.transform.parent);
        newGameObject.name = oldGameObject.name;
        Destroy(oldGameObject);
    }
}