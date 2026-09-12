using System;
using UnityEngine;

namespace Dusk.Utils;

[Serializable]
public class DungeonFlowReference
{
    [field: SerializeField]
    private string _flowAssetName;

    [field: SerializeField]
    private string _flowAssetGuid;

    [field: SerializeField]
    private string _bundleName;

    [field: SerializeField]
    private string[] _dungeonArchetypeNames = Array.Empty<string>();

    [field: SerializeField]
    private ArchetypeTileSetMapping[] _archetypeTileSets = Array.Empty<ArchetypeTileSetMapping>();

    [field: SerializeField]
    private GraphNodeReference[] _graphNodeReferences = Array.Empty<GraphNodeReference>();

    public string FlowAssetGuid => _flowAssetGuid;
    public string FlowAssetName => _flowAssetName;
    public string[] DungeonArchetypeNames => _dungeonArchetypeNames;
    public GraphNodeReference[] GraphNodeReferences => _graphNodeReferences;
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

        [field: SerializeField]
        private string[] _branchCapTileSetNames = Array.Empty<string>();

        public string ArchetypeName => _archetypeName;
        public string[] TileSetNames => _tileSetNames;
        public string[] BranchCapTileSetNames => _branchCapTileSetNames;
    }

    [Serializable]
    public class GraphNodeReference
    {
        [field: SerializeField]
        private string[] _tileSetNames;

        public string[] TileSetNames => _tileSetNames;
    }

    public bool TryGetTileSetsForArchetype(string archetypeName, out string[] branchCapTileSetNames, out string[] tileSetNames)
    {
        foreach (ArchetypeTileSetMapping mapping in _archetypeTileSets)
        {
            if (string.Equals(mapping.ArchetypeName, archetypeName, StringComparison.Ordinal))
            {
                tileSetNames = mapping.TileSetNames;
                branchCapTileSetNames = mapping.BranchCapTileSetNames;
                return true;
            }
        }

        tileSetNames = Array.Empty<string>();
        branchCapTileSetNames = Array.Empty<string>();
        return false;
    }
}