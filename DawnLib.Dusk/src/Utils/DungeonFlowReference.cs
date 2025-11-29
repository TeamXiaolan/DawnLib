using System;
using UnityEngine;

namespace Dusk.Utils;
[Serializable]
public class DungeonFlowReference
{
    [field: SerializeField]
    private string _flowAssetName;

    [field: SerializeField]
    private string _bundleName;

    [field: SerializeField]
    private string[] _dungeonArchetypeNames = Array.Empty<string>();

    [field: SerializeField]
    private string[] _tileSetNames = Array.Empty<string>();

    [field: SerializeField]
    private ArchetypeTileSetMapping[] _archetypeTileSets = Array.Empty<ArchetypeTileSetMapping>();

    public string FlowAssetName => _flowAssetName;
    public string[] DungeonArchetypeNames => _dungeonArchetypeNames;
    public string[] TileSetNames => _tileSetNames;

    public ArchetypeTileSetMapping[] ArchetypeTileSets => _archetypeTileSets;

    public string BundleName => _bundleName;

    public static implicit operator string(DungeonFlowReference reference)
    {
        return reference.FlowAssetName;
    }

    [Serializable]
    public class ArchetypeTileSetMapping
    {
        [field: SerializeField]
        private string _archetypeName;

        [field: SerializeField]
        private string[] _tileSetNames = Array.Empty<string>();

        public string ArchetypeName => _archetypeName;
        public string[] TileSetNames => _tileSetNames;
    }

    public bool TryGetTileSetsForArchetype(string archetypeName, out string[] tileSets)
    {
        foreach (var mapping in _archetypeTileSets)
        {
            if (string.Equals(mapping.ArchetypeName, archetypeName, StringComparison.Ordinal))
            {
                tileSets = mapping.TileSetNames;
                return true;
            }
        }

        tileSets = Array.Empty<string>();
        return false;
    }
}