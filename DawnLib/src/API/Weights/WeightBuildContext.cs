namespace Dawn;

public sealed class WeightBuildContext
{
    public Registry<DawnMoonInfo> Moons => LethalContent.Moons;

    public Registry<DawnDungeonInfo> Dungeons => LethalContent.Dungeons;

    public Registry<DawnWeatherEffectInfo> Weathers => LethalContent.Weathers;

    public Registry<DawnEnemyInfo> Enemies => LethalContent.Enemies;

    public Registry<DawnItemInfo> Items => LethalContent.Items;

    public Registry<DawnMapObjectInfo> MapObjects => LethalContent.MapObjects;
}