using Dawn.Internal;
using DunGen.Graph;

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
        SelectableLevel? level = StartOfRoundRefs.GetCurrentlLevel();
        DungeonFlow? dungeonFlow = RoundManagerRefs.GetCurrentDungeon();
        DawnWeatherEffectInfo? weather = TimeOfDayRefs.GetCurrentWeather();

        return this with
        {
            Moon = Moon ?? level?.DawnInfo,
            Dungeon = Dungeon ?? dungeonFlow?.DawnInfo,
            Weather = Weather ?? weather
        };
    }
}