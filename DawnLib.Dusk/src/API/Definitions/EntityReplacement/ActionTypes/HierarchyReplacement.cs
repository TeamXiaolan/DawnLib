using System.Collections;
using UnityEngine;

namespace Dusk;

public abstract class Hierarchy : ScriptableObject
{
    [field: SerializeField]
    public string HierarchyPath { get; private set; }

    public abstract IEnumerator Apply(Transform rootTransform);
}