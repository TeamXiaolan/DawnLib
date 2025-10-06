using UnityEngine;

namespace Dusk;

public abstract class HierarchyReplacement : ScriptableObject
{
    [field: SerializeField]
    public string HierarchyPath { get; private set; }

    public abstract void Apply(Transform rootTransform);
}