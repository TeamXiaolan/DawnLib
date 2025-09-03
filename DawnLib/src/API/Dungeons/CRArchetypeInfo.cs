using System.Collections.Generic;
using DunGen;

namespace CodeRebirthLib;
public class CRArchetypeInfo : CRBaseInfo<CRArchetypeInfo>
{
    public DungeonArchetype DungeonArchetype { get; }
    
    internal CRArchetypeInfo(NamespacedKey<CRArchetypeInfo> key, List<NamespacedKey> tags, DungeonArchetype archetype) : base(key, tags)
    {
        DungeonArchetype = archetype;
    }
    
    public void AddTileSet(CRTileSetInfo info)
    {
        if (LethalContent.Dungeons.IsFrozen) throw new RegistryFrozenException();
        _tileSets.Add(info);
    }
    
    private List<CRTileSetInfo> _tileSets = [];
    public IReadOnlyList<CRTileSetInfo> TileSets => _tileSets.AsReadOnly();
    
    public CRDungeonInfo ParentInfo { get; internal set; }
}