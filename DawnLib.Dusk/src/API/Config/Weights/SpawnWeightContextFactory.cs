using Dawn;

namespace Dusk.Weights;

public static class SpawnWeightContextFactory
{
    public static SpawnWeightContext FromCurrentGame()
    {
        DawnMoonInfo? moon = RoundManager.Instance?.currentLevel?.GetDawnInfo();
        DawnDungeonInfo? dungeon = RoundManager.Instance?.dungeonGenerator?.Generator?.DungeonFlow?.GetDawnInfo();

        DawnWeatherEffectInfo? weather = null;
        if (TimeOfDay.Instance?.currentLevel != null && TimeOfDay.Instance.currentLevel.currentWeather != LevelWeatherType.None)
        {
            weather = TimeOfDay.Instance.effects[(int)TimeOfDay.Instance.currentLevel.currentWeather].GetDawnInfo();
        }

        SpawnWeightExtras extras = SpawnWeightExtras.Empty;
        SelectableLevel? level = RoundManager.Instance?.currentLevel;
        if (level != null)
        {
            extras = extras.With(SpawnWeightExtraKeys.RoutingPriceKey, level.GetDawnInfo().DawnPurchaseInfo.Cost.Provide());
        }

        return new SpawnWeightContext(moon, dungeon, weather, extras);
    }

    public static SpawnWeightContext From(DawnMoonInfo? moon, DawnDungeonInfo? dungeon, DawnWeatherEffectInfo? weather, SpawnWeightExtras extras = default) => new(moon, dungeon, weather, extras);
}
