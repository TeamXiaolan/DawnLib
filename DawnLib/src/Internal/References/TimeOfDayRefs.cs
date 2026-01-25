namespace Dawn.Internal;

public static class TimeOfDayRefs
{
    private static TimeOfDay _instance;
    public static TimeOfDay Instance
    {
        get
        {
            if (_instance == null)
            {
                if (TimeOfDay.Instance != null)
                {
                    _instance = TimeOfDay.Instance;
                }
                else
                {
                    _instance = UnityEngine.Object.FindFirstObjectByType<TimeOfDay>();
                }
            }
            return _instance;
        }
    }

    /// <summary>
    /// Gets the current weather effect, or null if it is None.
    /// </summary>
    public static WeatherEffect? GetCurrentWeatherEffect(SelectableLevel? selectableLevel)
    {
        WeatherEffect? weather = null;
        if (selectableLevel != null && selectableLevel.currentWeather != LevelWeatherType.None)
        {
            weather = TimeOfDay.Instance.effects[(int)selectableLevel.currentWeather];
        }
        return weather;
    }
}