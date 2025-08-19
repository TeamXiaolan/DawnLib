using System.Collections.Generic;

namespace CodeRebirthLib;
public class CRMoonInfo : INamespaced<CRMoonInfo>, ITaggable
{
    private List<NamespacedKey> _tags;
    
    internal CRMoonInfo(NamespacedKey<CRMoonInfo> key, List<NamespacedKey> tags, SelectableLevel level)
    {
        TypedKey = key;
        Level = level;
        _tags = tags;
    }

    public SelectableLevel Level { get; }

    public NamespacedKey Key => TypedKey;
    public NamespacedKey<CRMoonInfo> TypedKey { get; }
    public bool HasTag(NamespacedKey tag)
    {
        return _tags.Contains(tag);
    }
}