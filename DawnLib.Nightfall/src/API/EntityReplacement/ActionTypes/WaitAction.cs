using System.Collections;
using UnityEngine;

namespace Nightfall;

[CreateAssetMenu(fileName = "New Wait Action", menuName = $"Entity Replacements/Actions/Wait Action")]
public class WaitAction : Hierarchy
{
    [field: SerializeField]
    public float WaitTime { get; private set; }

    public override IEnumerator Apply(Transform rootTransform, bool immediate = false)
    {
        if (!immediate)
        {
            yield return new WaitForSeconds(WaitTime);
        }
    }
}