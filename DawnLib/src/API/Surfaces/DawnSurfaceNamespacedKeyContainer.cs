using Dawn;
using UnityEngine;

public class DawnSurfaceNamespacedKeyContainer : MonoBehaviour
{
    [field: SerializeField]
    public NamespacedKey Value { get; internal set; }
}