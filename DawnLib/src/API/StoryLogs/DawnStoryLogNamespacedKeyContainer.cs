using Dawn;
using UnityEngine;

public class DawnStoryLogNamespacedKeyContainer : MonoBehaviour
{
    [field: SerializeField]
    public NamespacedKey Value { get; internal set; }
}