using Dawn.Internal;

namespace Dawn;

public readonly record struct WeightQuery
{
    public NamespacedKey Channel { get; init; }

    public object? Owner { get; init; }

    public object? Subject { get; init; }

    public DawnMoonInfo? Moon { get; init; }

    public DawnDungeonInfo? Dungeon { get; init; }

    public DawnWeatherEffectInfo? Weather { get; init; }

    public WeightQuery ResolveGameState()
    {
        DawnMoonInfo? moonInfo = StartOfRoundRefs.GetCurrentlLevelInfo();
        DawnDungeonInfo? dungeonInfo = RoundManagerRefs.GetCurrentDungeonInfo();
        DawnWeatherEffectInfo? weatherInfo = TimeOfDayRefs.GetCurrentWeather();

        return this with
        {
            Moon = Moon ?? moonInfo,
            Dungeon = Dungeon ?? dungeonInfo,
            Weather = Weather ?? weatherInfo
        };
    }
}