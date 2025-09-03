using System.Collections.Generic;
using CodeRebirthLib.Internal;

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
    public IEnumerable<NamespacedKey> AllTags()
    {
        return _tags;
    }

    /// <summary>
    /// Usually tags should be defined fully as the Info class is created. However, to make my life easier with applying tags
    /// to vanilla content, this method exists to add at a later point. 
    /// </summary>
    /// <param name="tag">new tag</param>
    internal void Internal_AddTag(NamespacedKey tag)
    {
        Debuggers.Tags?.Log($"Internal_AddTag: {tag} !!!");
        _tags.Add(tag);
    }
}