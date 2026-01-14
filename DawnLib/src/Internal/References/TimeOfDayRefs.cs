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
}