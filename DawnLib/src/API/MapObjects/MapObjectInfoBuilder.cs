using System;
using System.Collections.Generic;
using UnityEngine;

namespace Dawn;

public class MapObjectInfoBuilder : BaseInfoBuilder<CRMapObjectInfo, GameObject, MapObjectInfoBuilder>
{
    public class InsideBuilder
    {
        private MapObjectInfoBuilder _parentBuilder;

        // maybe replace this (vvv) with a SpawnableMapObject Builder?
        private bool _spawnFacingAwayFromWall, _spawnFacingWall, _spawnWWithBackToWall, _spawnWithBackFlushAgainstWall, _requireDistanceBetweenSpawns, _disallowSpawningNearEntrances; // this feels like it should be one SO or some data thing instead of a million bools
        private ProviderTable<AnimationCurve?, CRMoonInfo>? _weights;

        internal InsideBuilder(MapObjectInfoBuilder parent)
        {
            _parentBuilder = parent;
        }

        public InsideBuilder OverrideSpawnFacingAwayFromWall(bool spawnFacingAwayFromWall)
        {
            _spawnFacingAwayFromWall = spawnFacingAwayFromWall;
            return this;
        }

        public InsideBuilder OverrideSpawnFacingWall(bool spawnFacingWall)
        {
            _spawnFacingWall = spawnFacingWall;
            return this;
        }

        public InsideBuilder OverrideSpawnWithBackToWall(bool spawnWWithBackToWall)
        {
            _spawnWWithBackToWall = spawnWWithBackToWall;
            return this;
        }

        public InsideBuilder OverrideSpawnWithBackFlushAgainstWall(bool spawnWithBackFlushAgainstWall)
        {
            _spawnWithBackFlushAgainstWall = spawnWithBackFlushAgainstWall;
            return this;
        }

        public InsideBuilder OverrideRequireDistanceBetweenSpawns(bool requireDistanceBetweenSpawns)
        {
            _requireDistanceBetweenSpawns = requireDistanceBetweenSpawns;
            return this;
        }

        public InsideBuilder OverrideDisallowSpawningNearEntrances(bool disallowSpawningNearEntrances)
        {
            _disallowSpawningNearEntrances = disallowSpawningNearEntrances;
            return this;
        }

        public InsideBuilder SetWeights(Action<CurveTableBuilder<CRMoonInfo>> callback)
        {
            CurveTableBuilder<CRMoonInfo> builder = new CurveTableBuilder<CRMoonInfo>();
            callback(builder);
            _weights = builder.Build();
            return this;
        }

        internal CRInsideMapObjectInfo Build()
        {
            if (_weights == null)
            {
                CodeRebirthLibPlugin.Logger.LogWarning($"MapObject: '{_parentBuilder.key}' didn't set inside weights. If you intend to have no weights (doing something special), call .SetWeights(() => {{}})");
                _weights = ProviderTable<AnimationCurve?, CRMoonInfo>.Empty();
            }
            return new CRInsideMapObjectInfo(_weights, _spawnFacingAwayFromWall, _spawnFacingWall, _spawnWWithBackToWall, _spawnWithBackFlushAgainstWall, _requireDistanceBetweenSpawns, _disallowSpawningNearEntrances);
        }
    }

    public class OutsideBuilder
    {
        private MapObjectInfoBuilder _parentBuilder;

        private bool _alignWithTerrain;
        private ProviderTable<AnimationCurve?, CRMoonInfo>? _weights;

        internal OutsideBuilder(MapObjectInfoBuilder parent)
        {
            _parentBuilder = parent;
        }


        public OutsideBuilder OverrideAlignWithTerrain(bool alignWithTerrain)
        {
            _alignWithTerrain = alignWithTerrain;
            return this;
        }

        public OutsideBuilder SetWeights(Action<CurveTableBuilder<CRMoonInfo>> callback)
        {
            CurveTableBuilder<CRMoonInfo> builder = new CurveTableBuilder<CRMoonInfo>();
            callback(builder);
            _weights = builder.Build();
            return this;
        }

        internal CROutsideMapObjectInfo Build()
        {
            if (_weights == null)
            {
                CodeRebirthLibPlugin.Logger.LogWarning($"MapObject: '{_parentBuilder.key}' didn't set inside weights. If you intend to have no weights (doing something special), call .SetWeights(() => {{}})");
                _weights = ProviderTable<AnimationCurve?, CRMoonInfo>.Empty();
            }
            return new CROutsideMapObjectInfo(_weights, _alignWithTerrain);
        }
    }


    private CRInsideMapObjectInfo? _insideInfo;
    private CROutsideMapObjectInfo? _outsideInfo;

    internal MapObjectInfoBuilder(NamespacedKey<CRMapObjectInfo> key, GameObject mapObject) : base(key, mapObject)
    {
    }

    public MapObjectInfoBuilder DefineInside(Action<InsideBuilder> callback)
    {
        InsideBuilder builder = new(this);
        callback(builder);
        _insideInfo = builder.Build();
        return this;
    }

    public MapObjectInfoBuilder DefineOutside(Action<OutsideBuilder> callback)
    {
        OutsideBuilder builder = new(this);
        callback(builder);
        _outsideInfo = builder.Build();
        return this;
    }

    override internal CRMapObjectInfo Build()
    {
        return new CRMapObjectInfo(key, tags, value, _insideInfo, _outsideInfo);
    }
}