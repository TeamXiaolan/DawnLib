using System;

namespace CodeRebirthLib.CRMod;
[Serializable]
public class MapObjectData : EntityData<CRMMapObjectReference>
{
    public bool isInsideHazard;
    public bool createInsideHazardConfig;
    public string defaultInsideCurveSpawnWeights;
    public bool createInsideCurveSpawnWeightsConfig;
    public bool isOutsideHazard;
    public bool createOutsideHazardConfig;
    public string defaultOutsideCurveSpawnWeights;
    public bool createOutsideCurveSpawnWeightsConfig;
}