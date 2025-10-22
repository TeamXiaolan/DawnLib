using Dawn;
using UnityEngine;

public class DawnMapObjectNamespacedKeyContainer : MonoBehaviour
{
    [field: SerializeField]
    public NamespacedKey Value { get; internal set; }
}