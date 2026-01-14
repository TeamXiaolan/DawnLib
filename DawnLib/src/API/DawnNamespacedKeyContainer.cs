using UnityEngine;

namespace Dawn;
public class DawnNamespacedKeyContainer<T> : MonoBehaviour where T : NamespacedKey
{
    public T Value { get; internal set; }
}