using System;
using System.Collections.Generic;
using Dawn.Utils;
using DunGen;
using DunGen.Graph;
using UnityEngine;

namespace Dawn;
public class DungeonFlowInfoBuilder : BaseInfoBuilder<DawnDungeonInfo, DungeonFlow, DungeonFlowInfoBuilder>
{
    private float _mapTileSize = 0f;
    private AudioClip? _firstTimeAudio = null;
    private ProviderTable<int?, DawnMoonInfo, SpawnWeightContext> _weights;
    private string _assetBundlePath = string.Empty;
    private BoundedRange _dungeonRangeClamp = new BoundedRange(0, 0);
    private bool _stingerPlaysMoreThanOnce = false;
    private float _stingerPlayChance = 100f;
    private FuncProvider<bool> _allowStingerToPlay = new FuncProvider<bool>(() => true);

    internal DungeonFlowInfoBuilder(NamespacedKey<DawnDungeonInfo> key, DungeonFlow value) : base(key, value)
    {
    }

    public DungeonFlowInfoBuilder SetArchetypeTileSetMapping(string archetypeName, IEnumerable<string> tileSetNames)
    {
        GraphLine line = new GraphLine(value);
        value.Lines.Add(line);

        TileSet[] allExistingTileSets = value.GetUsedTileSets() ?? Array.Empty<TileSet>();
        var tileSetLookup = new Dictionary<string, TileSet>(StringComparer.Ordinal);

        foreach (var ts in allExistingTileSets)
        {
            if (!tileSetLookup.ContainsKey(ts.name))
            {
                tileSetLookup.Add(ts.name, ts);
            }
        }

        List<TileSet> tileSetsToUse = new();

        foreach (string rawName in tileSetNames)
        {
            string name = rawName.Trim();
            if (!tileSetLookup.TryGetValue(name, out var tileSet))
            {
                tileSet = ScriptableObject.CreateInstance<TileSet>();
                tileSet.name = name;
                tileSetLookup.Add(name, tileSet);
            }

            if (!tileSetsToUse.Contains(tileSet))
            {
                tileSetsToUse.Add(tileSet);
            }
        }

        DungeonArchetype? targetArchetype = null;
        var existingArchetypes = value.GetUsedArchetypes();

        if (existingArchetypes != null)
        {
            foreach (var archetype in existingArchetypes)
            {
                if (archetype == null) continue;

                if (string.Equals(archetype.name, archetypeName, StringComparison.Ordinal))
                {
                    targetArchetype = archetype;
                    break;
                }
            }
        }

        if (targetArchetype == null)
        {
            targetArchetype = ScriptableObject.CreateInstance<DungeonArchetype>();
            targetArchetype.name = archetypeName;
        }

        if (targetArchetype.TileSets == null)
        {
            targetArchetype.TileSets = new List<TileSet>();
        }
        else
        {
            targetArchetype.TileSets.Clear();
        }

        targetArchetype.TileSets.AddRange(tileSetsToUse);

        if (line.DungeonArchetypes == null)
        {
            line.DungeonArchetypes = new();
        }
        else
        {
            line.DungeonArchetypes.Clear();
        }

        line.DungeonArchetypes.Add(targetArchetype);

        return this;
    }

    public DungeonFlowInfoBuilder SetMapTileSize(float mapTileSize)
    {
        _mapTileSize = mapTileSize;
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

    public DungeonFlowInfoBuilder SetWeights(Action<WeightTableBuilder<DawnMoonInfo, SpawnWeightContext>> callback)
    {
        WeightTableBuilder<DawnMoonInfo, SpawnWeightContext> builder = new WeightTableBuilder<DawnMoonInfo, SpawnWeightContext>();
        callback(builder);
        _weights = builder.Build();
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
            _weights = ProviderTable<int?, DawnMoonInfo, SpawnWeightContext>.Empty();
        }

        DawnStingerDetail stingerDetail = new(_firstTimeAudio, _stingerPlaysMoreThanOnce, _stingerPlayChance, _allowStingerToPlay);
        return new DawnDungeonInfo(key, [], value, _weights, _mapTileSize, stingerDetail, _assetBundlePath, _dungeonRangeClamp, customData);
    }
}