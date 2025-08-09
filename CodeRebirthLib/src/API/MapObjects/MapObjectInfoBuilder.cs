using System.Collections.Generic;
using UnityEngine;

namespace CodeRebirthLib;

public class MapObjectInfoBuilder
{
    public class InsideBuilder
    {
        private MapObjectInfoBuilder _parentBuilder;

        private bool _spawnFacingAwayFromWall, _spawnFacingWall, _spawnWWithBackToWall, _spawnWithBackFlushAgainstWall, _requireDistanceBetweenSpawns, _disallowSpawningNearEntrances; // this feels like it should be one SO or some data thing instead of a million bools

        internal InsideBuilder(MapObjectInfoBuilder parent)
        {
            _parentBuilder = parent;
        }

        public InsideBuilder AddMoonAnimationCurve(NamespacedKey<CRMoonInfo> moon, AnimationCurve animationCurve)
        {
            if (_parentBuilder._animationCurveToLevelDict.TryGetValue(moon.ToString(), out _)) // TODO i really dont know how to get moon name from CRMoonInfo
            {
                _parentBuilder._animationCurveToLevelDict[moon.ToString()] = animationCurve;
            }
            _parentBuilder._animationCurveToLevelDict.Add(moon.ToString(), animationCurve);
            return this;
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

        public InsideBuilder OverrideSpawnWWithBackToWall(bool spawnWWithBackToWall)
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

        internal CRInsideMapObjectInfo Build()
        {
            return new CRInsideMapObjectInfo(_spawnFacingAwayFromWall, _spawnFacingWall, _spawnWWithBackToWall, _spawnWithBackFlushAgainstWall, _requireDistanceBetweenSpawns, _disallowSpawningNearEntrances);
        }
    }

    public class OutsideBuilder
    {
        private MapObjectInfoBuilder _parentBuilder;

        private bool _alignWithTerrain;

        internal OutsideBuilder(MapObjectInfoBuilder parent)
        {
            _parentBuilder = parent;
        }

        public OutsideBuilder AddMoonAnimationCurve(NamespacedKey<CRMoonInfo> moon, AnimationCurve animationCurve)
        {
            if (_parentBuilder._animationCurveToLevelDict.TryGetValue(moon.ToString(), out _)) // TODO i really dont know how to get moon name from CRMoonInfo
            {
                _parentBuilder._animationCurveToLevelDict[moon.ToString()] = animationCurve;
            }
            _parentBuilder._animationCurveToLevelDict.Add(moon.ToString(), animationCurve);
            return this;
        }

        public OutsideBuilder OverrideAlignWithTerrain(bool alignWithTerrain)
        {
            _alignWithTerrain = alignWithTerrain;
            return this;
        }

        internal CROutsideMapObjectInfo Build()
        {
            return new CROutsideMapObjectInfo(_alignWithTerrain);
        }
    }

    private NamespacedKey<CRMapObjectInfo> _key;
    private GameObject _mapObject;
    private Dictionary<string, AnimationCurve> _animationCurveToLevelDict = new();

    private CRInsideMapObjectInfo? _insideInfo;
    private CROutsideMapObjectInfo? _outsideInfo;

    internal MapObjectInfoBuilder(NamespacedKey<CRMapObjectInfo> key, GameObject mapObject, Dictionary<string, AnimationCurve> animationCurveToLevelDict)
    {
        _key = key;
        _mapObject = mapObject;
        _animationCurveToLevelDict = animationCurveToLevelDict;
    }

    internal CRMapObjectInfo Build()
    {
        return new CRMapObjectInfo(_key, _mapObject, _animationCurveToLevelDict, _insideInfo, _outsideInfo);
    }
}