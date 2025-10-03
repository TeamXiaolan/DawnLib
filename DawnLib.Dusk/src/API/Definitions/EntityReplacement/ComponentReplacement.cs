using UnityEngine;

namespace Dusk;

public abstract class ComponentReplacement<T> : ScriptableObject where T : Component
{
    [field: SerializeField]
    public string PathToComponent { get; private set; }
    [field: SerializeField]
    public T Component { get; private set; }
}