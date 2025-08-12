using DunGen.Graph;

namespace CodeRebirthLib;
public class CRDungeonInfo : INamespaced<CRDungeonInfo>, ITaggable
{
    internal CRDungeonInfo(NamespacedKey<CRDungeonInfo> key, DungeonFlow flow)
    {
        TypedKey = key;
        Flow = flow;
    }
    
    public DungeonFlow Flow { get; }
    public NamespacedKey Key => TypedKey;
    public NamespacedKey<CRDungeonInfo> TypedKey { get; }
    
    public bool HasTag(NamespacedKey tag)
    {
        return false; // todo
    }
}