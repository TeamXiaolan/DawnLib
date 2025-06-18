using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using CodeRebirthLib.AssetManagement;
using System.Linq;
using UnityEngine;

namespace CodeRebirthLib.ContentManagement;
public abstract class CRRegistry
{
}

public class CRRegistry<TDefinition> : CRRegistry, IEnumerable<TDefinition> where TDefinition : CRContentDefinition
{
    [SerializeField]
    private List<TDefinition> _items = new();

    public void Register(TDefinition item)
    {
        _items.Add(item);
        CodeRebirthLibPlugin.ExtendedLogging($"added {item.name} to registry.");
    }

    public bool TryGetFirstBySomeName(Func<TDefinition, string> transformer, string name, [NotNullWhen(true)] out TDefinition? value)
    {
        value = this.FirstOrDefault(it => transformer(it).Contains(name, StringComparison.OrdinalIgnoreCase));
        return value;// implicit cast to bool
    }
    
    public bool TryGetFromAssetName(string name, [NotNullWhen(true)] out TDefinition? value)
    {
        return TryGetFirstBySomeName(it => it.name, name, out value);
    }
    
    public IEnumerator<TDefinition> GetEnumerator()
    {
        return _items.GetEnumerator();
    }
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}