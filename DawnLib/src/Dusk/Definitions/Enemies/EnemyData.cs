using System;

namespace Dawn.Dusk;

[Serializable]
public class EnemyData : EntityData<DuskEnemyReference>
{
    public string moonSpawnWeights;
    public string interiorSpawnWeights;
    public string weatherSpawnWeights;
    public bool generateSpawnWeightsConfig;
    public float powerLevel;
    public int maxSpawnCount;
}