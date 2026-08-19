namespace Dawn;

public sealed class DawnEnemyLocationInfo
{
    public DawnEnemyInfo ParentInfo { get; internal set; }

    internal DawnEnemyLocationInfo(DawnWeightedValue<int> rarity)
    {
        Rarity = rarity;
    }

    public DawnWeightedValue<int> Rarity { get; }

    public int GetRarity(DawnMoonInfo? moonInfo = null, DawnDungeonInfo? dungeonInfo = null, DawnWeatherEffectInfo? weatherEffectInfo = null, bool resolveAutomatically = true)
    {
        return Rarity.GetValue(new WeightQuery
        {
            Owner = ParentInfo,
            Subject = this,
            Moon = moonInfo,
            Dungeon = dungeonInfo,
            Weather = weatherEffectInfo,
            Channel = DawnWeightChannels.EnemyRarity.Key,
            ResolveAutomatically = resolveAutomatically
        });
    }
}