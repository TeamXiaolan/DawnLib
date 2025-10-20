using System.Collections;
using UnityEngine;

namespace Dusk;

[CreateAssetMenu(fileName = "New Wait Action", menuName = $"Entity Replacements/Actions/Wait Action")]
public class WaitAction : Hierarchy
{
    [field: SerializeField]
    public float WaitTime { get; private set; }

    [field: HideInInspector]
    public new string HierarchyPath { get; private set; }

    public override IEnumerator Apply(Transform rootTransform)
    {
        yield return new WaitForSeconds(WaitTime);
    }
}