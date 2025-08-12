namespace CodeRebirthLib;
public class CRMoonInfo : INamespaced<CRMoonInfo>, ITaggable
{
    internal CRMoonInfo(NamespacedKey<CRMoonInfo> key, SelectableLevel level)
    {
        TypedKey = key;
        Level = level;
    }
    
    public SelectableLevel Level { get; }
    
    public NamespacedKey Key => TypedKey;
    public NamespacedKey<CRMoonInfo> TypedKey { get; }
    public bool HasTag(NamespacedKey tag)
    {
        return false; // todo
    }
}