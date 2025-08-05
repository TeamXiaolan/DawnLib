using System;
using UnityEngine;

namespace CodeRebirthLib.ContentManagement.MapObjects;
[Serializable]
public class MapObjectData : EntityData
{
    [SerializeReference]
    public CRMapObjectReference mapObjectReference = new(string.Empty);
    public override string EntityName => mapObjectReference.entityName;

    public bool isInsideHazard;
    public bool createInsideHazardConfig;
    public string defaultInsideCurveSpawnWeights;
    public bool createInsideCurveSpawnWeightsConfig;
    public bool isOutsideHazard;
    public bool createOutsideHazardConfig;
    public string defaultOutsideCurveSpawnWeights;
    public bool createOutsideCurveSpawnWeightsConfig;
}