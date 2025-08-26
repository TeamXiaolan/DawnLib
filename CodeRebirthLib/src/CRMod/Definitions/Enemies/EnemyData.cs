using System;
using UnityEngine;

namespace CodeRebirthLib.CRMod;

[Serializable]
public class EnemyData : EntityData<CRMEnemyReference>, IInspectorHeaderWarning
{
    public bool TryGetHeaderWarning(out string? message)
    {
        message = null;
        return false;
    }

    public string moonSpawnWeights;
    public string interiorSpawnWeights;
    public string weatherSpawnWeights;
    public bool generateSpawnWeightsConfig;
    public float powerLevel;
    public int maxSpawnCount;
}