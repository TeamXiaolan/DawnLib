using System;
using UnityEngine;

namespace Dawn;

public class MapObjectInfoBuilder : BaseInfoBuilder<DawnMapObjectInfo, GameObject, MapObjectInfoBuilder>
{
    public class InsideBuilder
    {
        private MapObjectInfoBuilder _parentBuilder;

        // maybe replace this (vvv) with a SpawnableMapObject Builder?
        private bool _spawnFacingAwayFromWall, _spawnFacingWall, _spawnWithBackToWall, _spawnWithBackFlushAgainstWall, _requireDistanceBetweenSpawns, _disallowSpawningNearEntrances, _allowInMineshaft; // this feels like it should be one SO or some data thing instead of a million bools
        private ProviderTable<AnimationCurve?, DawnMoonInfo, SpawnWeightContext>? _weights;

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
            _spawnWithBackToWall = spawnWWithBackToWall;
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

        public InsideBuilder OverrideAllowInMineshaft(bool allowInMineshaft)
        {
            _allowInMineshaft = allowInMineshaft;
            return this;
        }

        public InsideBuilder SetWeights(Action<CurveTableBuilder<DawnMoonInfo, SpawnWeightContext>> callback)
        {
            CurveTableBuilder<DawnMoonInfo, SpawnWeightContext> builder = new CurveTableBuilder<DawnMoonInfo, SpawnWeightContext>();
            callback(builder);
            _weights = builder.Build();
            return this;
        }

        internal DawnInsideMapObjectInfo Build()
        {
            if (_weights == null)
            {
                DawnPlugin.Logger.LogWarning($"MapObject: '{_parentBuilder.key}' didn't set inside weights. If you intend to have no weights (doing something special), call .SetWeights(() => {{}})");
                _weights = ProviderTable<AnimationCurve?, DawnMoonInfo, SpawnWeightContext>.Empty();
            }

            IndoorMapHazardType indoorMapHazardType = ScriptableObject.CreateInstance<IndoorMapHazardType>();
            indoorMapHazardType.spawnFacingAwayFromWall = _spawnFacingAwayFromWall;
            indoorMapHazardType.spawnFacingWall = _spawnFacingWall;
            indoorMapHazardType.spawnWithBackToWall = _spawnWithBackToWall;
            indoorMapHazardType.spawnWithBackFlushAgainstWall = _spawnWithBackFlushAgainstWall;
            indoorMapHazardType.requireDistanceBetweenSpawns = _requireDistanceBetweenSpawns;
            indoorMapHazardType.disallowSpawningNearEntrances = _disallowSpawningNearEntrances;
            indoorMapHazardType.allowInMineshaft = _allowInMineshaft;
            return new DawnInsideMapObjectInfo(indoorMapHazardType, _weights);
        }
    }

    public class OutsideBuilder
    {
        private MapObjectInfoBuilder _parentBuilder;

        private bool _alignWithTerrain, _destroyTrees, _spawnFacingAwayFromWall = false;
        private int _objectWidth = 6, _minimumNodeSpawnRequirement = 0;
        private Vector3 _rotationOffset = Vector3.zero;
        private string[] _spawnableFloorTags = Array.Empty<string>();

        private ProviderTable<AnimationCurve?, DawnMoonInfo, SpawnWeightContext>? _weights;

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

        public OutsideBuilder OverrideDestroyTrees(bool destroyTrees)
        {
            _destroyTrees = destroyTrees;
            return this;
        }

        public OutsideBuilder SetWeights(Action<CurveTableBuilder<DawnMoonInfo, SpawnWeightContext>> callback)
        {
            CurveTableBuilder<DawnMoonInfo, SpawnWeightContext> builder = new CurveTableBuilder<DawnMoonInfo, SpawnWeightContext>();
            callback(builder);
            _weights = builder.Build();
            return this;
        }

        internal DawnOutsideMapObjectInfo Build()
        {
            if (_weights == null)
            {
                DawnPlugin.Logger.LogWarning($"MapObject: '{_parentBuilder.key}' didn't set inside weights. If you intend to have no weights (doing something special), call .SetWeights(() => {{}})");
                _weights = ProviderTable<AnimationCurve?, DawnMoonInfo, SpawnWeightContext>.Empty();
            }

            SpawnableOutsideObject spawnableOutsideObject = ScriptableObject.CreateInstance<SpawnableOutsideObject>();
            spawnableOutsideObject.spawnFacingAwayFromWall = _spawnFacingAwayFromWall;
            spawnableOutsideObject.rotationOffset = _rotationOffset;
            spawnableOutsideObject.destroyTrees = _destroyTrees;
            spawnableOutsideObject.spawnableFloorTags = _spawnableFloorTags;
            spawnableOutsideObject.objectWidth = _objectWidth;
            return new DawnOutsideMapObjectInfo(spawnableOutsideObject, _weights, _alignWithTerrain, _minimumNodeSpawnRequirement);
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
        return new DawnMapObjectInfo(key, tags, _insideInfo, _outsideInfo, customData);
    }
}