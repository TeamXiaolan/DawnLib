using System.Linq;
using UnityEngine;

namespace CodeRebirthLib;

static class MapObjectRegistrationHandler
{
    internal static void Init()
    {
        On.StartOfRound.Awake += RegisterMapObjects;
    }

    private static void RegisterMapObjects(On.StartOfRound.orig_Awake orig, StartOfRound self)
    {
        foreach (var level in self.levels)
        {
            foreach (var mapObjectInfo in LethalContent.MapObjects.Values) // TODO, do registration for outside map objects as well
            {
                if (mapObjectInfo.InsideInfo == null || mapObjectInfo.Key.IsVanilla())
                    continue;

                AnimationCurve animationCurve = mapObjectInfo.AnimationCurveToLevelDict[level.name]; // convert to the LLL format
                if (animationCurve == AnimationCurve.Linear(0, 0, 1, 0))
                    continue;

                SpawnableMapObject spawnableMapObject = new()
                {
                    prefabToSpawn = mapObjectInfo.MapObject,
                    spawnFacingAwayFromWall = mapObjectInfo.InsideInfo.SpawnFacingAwayFromWall,
                    spawnFacingWall = mapObjectInfo.InsideInfo.SpawnFacingWall,
                    spawnWithBackFlushAgainstWall = mapObjectInfo.InsideInfo.SpawnWithBackFlushAgainstWall,
                    spawnWithBackToWall = mapObjectInfo.InsideInfo.SpawnWithBackToWall,
                    requireDistanceBetweenSpawns = mapObjectInfo.InsideInfo.RequireDistanceBetweenSpawns,
                    disallowSpawningNearEntrances = mapObjectInfo.InsideInfo.DisallowSpawningNearEntrances,
                    numberToSpawn = animationCurve
                };

                level.spawnableMapObjects = level.spawnableMapObjects.Append(spawnableMapObject).ToArray();
            }
        }

        // then, before freezing registry, add vanilla content
        if (!LethalContent.MapObjects.IsFrozen) // effectively check for a lobby reload
        {
            // todo?
        }

        LethalContent.MapObjects.Freeze();
        orig(self);
    }
}