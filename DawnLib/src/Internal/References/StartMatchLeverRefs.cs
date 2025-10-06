namespace Dawn.Internal;

public static class StartMatchLeverRefs
{
    private static StartMatchLever _instance;
    public static StartMatchLever Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = UnityEngine.Object.FindFirstObjectByType<StartMatchLever>();
            }
            return _instance;
        }
    }
}