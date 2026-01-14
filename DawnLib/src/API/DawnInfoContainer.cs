using UnityEngine;

namespace Dawn;
public class DawnInfoContainer<T> : MonoBehaviour where T : DawnBaseInfo<T>
{
    public T Value { get; internal set; }
}