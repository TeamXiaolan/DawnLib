using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CodeRebirthLib.ContentManagement;
public abstract class CRRegistry
{
}

public class CRRegistry<TDefinition> : CRRegistry, IEnumerable<TDefinition> where TDefinition : CRContentDefinition
{
    [SerializeField]
    private readonly List<TDefinition> _items = new();

    public IEnumerator<TDefinition> GetEnumerator()
    {
        return _items.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Register(TDefinition item)
    {
        _items.Add(item);
        CodeRebirthLibPlugin.ExtendedLogging($"added {item.name} to registry.");
    }
}