using System;
using System.Collections.Generic;
using System.Linq;
using Dawn.Utils;
using DunGen;
using DunGen.Graph;
using UnityEngine;

namespace Dawn;

public class DungeonFlowInfoBuilder : BaseInfoBuilder<DawnDungeonInfo, DungeonFlow, DungeonFlowInfoBuilder>
{
    private float _mapTileSize = 0f;
    private AudioClip? _firstTimeAudio = null;
    private DawnWeightedValue<int> _weights;
    private string _assetBundlePath = string.Empty;
    private BoundedRange _dungeonRangeClamp = new BoundedRange(0, 999);
    private bool _stingerPlaysMoreThanOnce = false;
    private float _stingerPlayChance = 100f;
    private FuncProvider<bool> _allowStingerToPlay = new FuncProvider<bool>(() => true);
    private int _extraScrapGeneration;

    private List<TileSet> _tileSetsCreated = new();

    internal DungeonFlowInfoBuilder(NamespacedKey<DawnDungeonInfo> key, DungeonFlow value) : base(key, value)
    {
    }

    public DungeonFlowInfoBuilder SetArchetypeTileSetsMapping(string archetypeName, IEnumerable<string> branchCapTileSetNames, IEnumerable<string> tileSetNames)
    {
        GraphLine line = new GraphLine(value);
        value.Lines.Add(line);

        HashSet<TileSet> branchCapTileSets = new();
        foreach (string rawName in branchCapTileSetNames)
        {
            string name = rawName.Trim();
            if (_tileSetsCreated.Exists(tileSet => tileSet.name == name))
            {
                branchCapTileSets.Add(_tileSetsCreated.First(tileSet => tileSet.name == name));
                continue;
            }
            TileSet tileSet = ScriptableObject.CreateInstance<TileSet>();
            tileSet.name = name;
            branchCapTileSets.Add(tileSet);
            _tileSetsCreated.Add(tileSet);
        }

        HashSet<TileSet> tileSets = new();
        foreach (string rawName in tileSetNames)
        {
            string name = rawName.Trim();
            if (_tileSetsCreated.Exists(tileSet => tileSet.name == name))
            {
                tileSets.Add(_tileSetsCreated.First(tileSet => tileSet.name == name));
                continue;
            }
            TileSet tileSet = ScriptableObject.CreateInstance<TileSet>();
            tileSet.name = name;
            tileSets.Add(tileSet);
            _tileSetsCreated.Add(tileSet);
        }

        DungeonArchetype targetArchetype = ScriptableObject.CreateInstance<DungeonArchetype>();
        targetArchetype.name = archetypeName;
        targetArchetype.TileSets = [.. tileSets];
        targetArchetype.BranchCapTileSets = [.. branchCapTileSets];
        line.DungeonArchetypes = [targetArchetype];
        return this;
    }

    public void SetTileSet(IEnumerable<string> tileSetNames)
    {
        GraphNode node = new GraphNode(value);
        value.Nodes.Add(node);
        node.TileSets = [];

        foreach (string tileSetName in tileSetNames)
        {
            string name = tileSetName.Trim();
            if (_tileSetsCreated.Exists(tileSet => tileSet.name == name))
            {
                node.TileSets.Add(_tileSetsCreated.Find(tileSet => tileSet.name == name));
                continue;
            }

            TileSet tileSet = ScriptableObject.CreateInstance<TileSet>();
            tileSet.name = name;
            node.TileSets.Add(tileSet);
            _tileSetsCreated.Add(tileSet);
        }
    }

    public DungeonFlowInfoBuilder SetMapTileSize(float mapTileSize)
    {
        _mapTileSize = mapTileSize;
        return this;
    }

    public DungeonFlowInfoBuilder SetExtraScrapGeneration(int extraScrapGeneration)
    {
        _extraScrapGeneration = extraScrapGeneration;
        return this;
    }

    public DungeonFlowInfoBuilder SetAssetBundlePath(string assetBundlePath)
    {
        _assetBundlePath = assetBundlePath;
        return this;
    }

    public DungeonFlowInfoBuilder SetFirstTimeAudio(AudioClip firstTimeAudio)
    {
        _firstTimeAudio = firstTimeAudio;
        return this;
    }

    public DungeonFlowInfoBuilder OverrideStingerPlaysMoreThanOnce(bool stingerPlaysMoreThanOnce)
    {
        _stingerPlaysMoreThanOnce = stingerPlaysMoreThanOnce;
        return this;
    }

    public DungeonFlowInfoBuilder OverrideStingerPlayChance(float stingerPlayChance)
    {
        _stingerPlayChance = stingerPlayChance;
        return this;
    }

    public DungeonFlowInfoBuilder SetWeights(Action<WeightProfile<int>> callback)
    {
        WeightProfile<int> profile = new WeightProfile<int>(DawnWeightChannels.DungeonRarity.Policy);
        callback(profile);
        _weights = new DawnWeightedValue<int>(DawnWeightChannels.DungeonRarity, profile);
        return this;
    }

    public DungeonFlowInfoBuilder SetDungeonRangeClamp(BoundedRange dungeonRangeClamp)
    {
        _dungeonRangeClamp = dungeonRangeClamp;
        return this;
    }

    public DungeonFlowInfoBuilder OverrideAllowStingerToPlay(FuncProvider<bool> allowStingerToPlay)
    {
        _allowStingerToPlay = allowStingerToPlay;
        return this;
    }

    override internal DawnDungeonInfo Build()
    {
        if (_weights == null)
        {
            DawnPlugin.Logger.LogWarning($"DungeonFlow '{key}' didn't set weights. If you intend to have no weights (doing something special), call .SetWeights(() => {{}})");
            _weights = new DawnWeightedValue<int>(DawnWeightChannels.DungeonRarity);
        }

        DawnStingerDetail stingerDetail = new(_firstTimeAudio, _stingerPlaysMoreThanOnce, _stingerPlayChance, _allowStingerToPlay);
        DawnDungeonInfo dungeonInfo = new DawnDungeonInfo(key, tags, value, _weights, _mapTileSize, stingerDetail, _assetBundlePath, _dungeonRangeClamp, _extraScrapGeneration, customData);

        foreach (DungeonArchetype archetype in value.GetUsedArchetypes())
        {
            NamespacedKey<DawnArchetypeInfo> archetypeKey = NamespacedKey<DawnArchetypeInfo>.From(dungeonInfo.Key.Namespace, archetype.name);
            DawnArchetypeInfo archetypeInfo = new DawnArchetypeInfo(archetypeKey, tags, archetype, null);
            archetype.DawnInfo = archetypeInfo;
            archetypeInfo.ParentInfo = dungeonInfo;
            LethalContent.Archetypes.Register(archetypeInfo);
            foreach (TileSet tileSet in archetype.TileSets)
            {
                NamespacedKey<DawnTileSetInfo> tileSetKey = NamespacedKey<DawnTileSetInfo>.From(dungeonInfo.Key.Namespace, tileSet.name);
                DawnTileSetInfo tileSetInfo = new DawnTileSetInfo(tileSetKey, tags, ConstantPredicate.True, tileSet, archetypeInfo.DungeonArchetype.BranchCapTileSets.Contains(tileSet), archetypeInfo.DungeonArchetype.TileSets.Contains(tileSet), null);
                archetypeInfo.AddTileSet(tileSetInfo);
                tileSet.DawnInfo = tileSetInfo;
                LethalContent.TileSets.Register(tileSetInfo);
            }
        }

        return dungeonInfo;
    }
}