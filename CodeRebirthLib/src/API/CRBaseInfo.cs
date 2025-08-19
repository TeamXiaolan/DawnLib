using System;
using System.Collections.Generic;
using System.Reflection;

namespace CodeRebirthLib;
public abstract class CRBaseInfo<T> : INamespaced<T>, ITaggable where T : CRBaseInfo<T>
{
    private List<NamespacedKey> _tags;
    
    protected CRBaseInfo(NamespacedKey<T> key, List<NamespacedKey> tags)
    {
        TypedKey = key;
        _tags = tags;
    }

    public NamespacedKey Key => TypedKey;
    public NamespacedKey<T> TypedKey { get; }
    public bool HasTag(NamespacedKey tag)
    {
        return _tags.Contains(tag);
    }
}