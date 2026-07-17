using Dawn;
using UnityEngine;

public class DawnDeadBodyNamespacedKeyContainer : MonoBehaviour
{
    [field: SerializeField]
    public NamespacedKey Value { get; internal set; }

    public DawnDeadBodyInfo GetDawnDeadBodynfo()
    {
        return LethalContent.DeadBodies[Value.AsTyped<DawnDeadBodyInfo>()];
    }
}