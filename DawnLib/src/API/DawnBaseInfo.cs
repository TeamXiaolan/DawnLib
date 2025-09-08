using System.Collections.Generic;
using Dawn.Internal;

namespace Dawn;
public abstract class DawnBaseInfo<T> : INamespaced<T>, ITaggable where T : DawnBaseInfo<T>
{
    private List<NamespacedKey> _tags;

    private IDataContainer? _customData;

    public IDataContainer CustomData
    {
        get
        {
            if (_customData == null) _customData = new DataContainer();
            return _customData;
        }
    }

    protected DawnBaseInfo(NamespacedKey<T> key, List<NamespacedKey> tags, IDataContainer? customData)
    {
        TypedKey = key;
        _tags = tags;
        _customData = customData;
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