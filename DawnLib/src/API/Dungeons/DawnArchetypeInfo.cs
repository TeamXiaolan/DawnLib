using System.Collections.Generic;
using DunGen;

namespace Dawn;
public class DawnArchetypeInfo : DawnBaseInfo<DawnArchetypeInfo>
{
    public DungeonArchetype DungeonArchetype { get; }

    internal DawnArchetypeInfo(NamespacedKey<DawnArchetypeInfo> key, List<NamespacedKey> tags, DungeonArchetype archetype, DataContainer? customData) : base(key, tags, customData)
    {
        DungeonArchetype = archetype;
    }

    public void AddTileSet(DawnTileSetInfo info)
    {
        if (LethalContent.Dungeons.IsFrozen) throw new RegistryFrozenException();
        _tileSets.Add(info);
    }

    private List<DawnTileSetInfo> _tileSets = [];
    public IReadOnlyList<DawnTileSetInfo> TileSets => _tileSets.AsReadOnly();

    public DawnDungeonInfo ParentInfo { get; internal set; }
}