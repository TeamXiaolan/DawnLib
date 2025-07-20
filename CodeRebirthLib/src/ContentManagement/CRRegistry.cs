using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CodeRebirthLib.AssetManagement;
using CodeRebirthLib.Extensions;
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

    [Obsolete("Use TryGetFirstBySomeName from CRRegistryExtensions instead")]
    public bool TryGetFirstBySomeName(Func<TDefinition, string> transformer, string name, [NotNullWhen(true)] out TDefinition? value, string? failContext = null)
    {
        value = this.FirstOrDefault(it => transformer(it).Contains(name, StringComparison.OrdinalIgnoreCase));
        if (!value && !string.IsNullOrEmpty(failContext))
        {
            CodeRebirthLibPlugin.ExtendedLogging(failContext);
        }
        return value; // implicit cast to bool
    }

    public bool TryGetFromAssetName(string name, [NotNullWhen(true)] out TDefinition? value)
    {
        return CRRegistryExtensions.TryGetFirstBySomeName(this, it => it.name, name, out value);
    }
}