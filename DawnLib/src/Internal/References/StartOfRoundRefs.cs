namespace Dawn.Internal;

static class StartOfRoundRefs
{
    private static StartOfRound _instance;
    public static StartOfRound Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = UnityEngine.Object.FindFirstObjectByType<StartOfRound>();
            }
            return _instance;
        }
    }
}