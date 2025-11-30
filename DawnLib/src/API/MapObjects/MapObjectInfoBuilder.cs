using System;
using UnityEngine;

namespace Dawn;

public class MapObjectInfoBuilder : BaseInfoBuilder<DawnMapObjectInfo, GameObject, MapObjectInfoBuilder>
{
    public class InsideBuilder
    {
        private MapObjectInfoBuilder _parentBuilder;

        // maybe replace this (vvv) with a SpawnableMapObject Builder?
        private bool _spawnFacingAwayFromWall, _spawnFacingWall, _spawnWWithBackToWall, _spawnWithBackFlushAgainstWall, _requireDistanceBetweenSpawns, _disallowSpawningNearEntrances; // this feels like it should be one SO or some data thing instead of a million bools
        private ProviderTable<AnimationCurve?, DawnMoonInfo>? _weights;

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

        public InsideBuilder SetWeights(Action<CurveTableBuilder<DawnMoonInfo>> callback)
        {
            CurveTableBuilder<DawnMoonInfo> builder = new CurveTableBuilder<DawnMoonInfo>();
            callback(builder);
            _weights = builder.Build();
            return this;
        }

        internal DawnInsideMapObjectInfo Build()
        {
            if (_weights == null)
            {
                DawnPlugin.Logger.LogWarning($"MapObject: '{_parentBuilder.key}' didn't set inside weights. If you intend to have no weights (doing something special), call .SetWeights(() => {{}})");
                _weights = ProviderTable<AnimationCurve?, DawnMoonInfo>.Empty();
            }
            return new DawnInsideMapObjectInfo(_weights, _spawnFacingAwayFromWall, _spawnFacingWall, _spawnWWithBackToWall, _spawnWithBackFlushAgainstWall, _requireDistanceBetweenSpawns, _disallowSpawningNearEntrances);
        }
    }

    public class OutsideBuilder
    {
        private MapObjectInfoBuilder _parentBuilder;

        private bool _alignWithTerrain, _spawnFacingAwayFromWall = false;
        private int _objectWidth = 6, _minimumNodeSpawnRequirement = 0;
        private Vector3 _rotationOffset = Vector3.zero;
        private string[] _spawnableFloorTags = Array.Empty<string>();

        private ProviderTable<AnimationCurve?, DawnMoonInfo>? _weights;

        internal OutsideBuilder(MapObjectInfoBuilder parent)
        {
            _parentBuilder = parent;
        }

        public OutsideBuilder OverrideAlignWithTerrain(bool alignWithTerrain)
        {
            _alignWithTerrain = alignWithTerrain;
            return this;
        }

        public OutsideBuilder OverrideSpawnFacingAwayFromWall(bool spawnFacingAwayFromWall)
        {
            _spawnFacingAwayFromWall = spawnFacingAwayFromWall;
            return this;
        }

        public OutsideBuilder OverrideMinimumNodeSpawnRequirement(int minimumAINodeRequirement)
        {
            _minimumNodeSpawnRequirement = minimumAINodeRequirement;
            return this;
        }

        public OutsideBuilder OverrideRotationOffset(Vector3 rotationOffset)
        {
            _rotationOffset = rotationOffset;
            return this;
        }

        public OutsideBuilder OverrideSpawnableFloorTags(string[] spawnableFloorTags)
        {
            _spawnableFloorTags = spawnableFloorTags;
            return this;
        }

        public OutsideBuilder OverrideObjectWidth(int objectWidth)
        {
            _objectWidth = objectWidth;
            return this;
        }

        public OutsideBuilder SetWeights(Action<CurveTableBuilder<DawnMoonInfo>> callback)
        {
            CurveTableBuilder<DawnMoonInfo> builder = new CurveTableBuilder<DawnMoonInfo>();
            callback(builder);
            _weights = builder.Build();
            return this;
        }

        internal DawnOutsideMapObjectInfo Build()
        {
            if (_weights == null)
            {
                DawnPlugin.Logger.LogWarning($"MapObject: '{_parentBuilder.key}' didn't set inside weights. If you intend to have no weights (doing something special), call .SetWeights(() => {{}})");
                _weights = ProviderTable<AnimationCurve?, DawnMoonInfo>.Empty();
            }
            return new DawnOutsideMapObjectInfo(_weights, _spawnFacingAwayFromWall, _objectWidth, _spawnableFloorTags, _rotationOffset, _alignWithTerrain, _minimumNodeSpawnRequirement);
        }
    }

    private DawnInsideMapObjectInfo? _insideInfo;
    private DawnOutsideMapObjectInfo? _outsideInfo;

    internal MapObjectInfoBuilder(NamespacedKey<DawnMapObjectInfo> key, GameObject mapObject) : base(key, mapObject)
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

    override internal DawnMapObjectInfo Build()
    {
        return new DawnMapObjectInfo(key, tags, value, _insideInfo, _outsideInfo, customData);
    }
}