using System;
using UnityEngine;

namespace CodeRebirthLib.ContentManagement.MapObjects;

public class RegisteredCRMapObject<TMapObject>(TMapObject mapObject, bool alignWithTerrain, bool hasNetworkObject, Func<SelectableLevel, AnimationCurve> spawnRateFunction)
{
    public TMapObject MapObject { get; } = mapObject;
    public bool AlignWithTerrain { get; } = alignWithTerrain; // this isn't a thing for inside map object, should this class be separate entirely?
    public bool HasNetworkObject { get; } = hasNetworkObject; // this is REQUIRED  for an inside map object
    public Func<SelectableLevel, AnimationCurve> SpawnRateFunction { get; } = spawnRateFunction;
}