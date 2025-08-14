using DunGen.Graph;

namespace CodeRebirthLib;
public class CRDungeonInfo : INamespaced<CRDungeonInfo>, ITaggable
{
    internal CRDungeonInfo(NamespacedKey<CRDungeonInfo> key, DungeonFlow dungeonFlow)
    {
        TypedKey = key;
        DungeonFlow = dungeonFlow;
    }

    public DungeonFlow DungeonFlow { get; }
    public NamespacedKey Key => TypedKey;
    public NamespacedKey<CRDungeonInfo> TypedKey { get; }

    public bool HasTag(NamespacedKey tag)
    {
        return false; // todo
    }
}