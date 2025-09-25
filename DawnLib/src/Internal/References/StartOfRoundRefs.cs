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
                if (StartOfRound.Instance != null)
                {
                    _instance = StartOfRound.Instance;
                }
                else
                {
                    _instance = UnityEngine.Object.FindFirstObjectByType<StartOfRound>();
                }
            }
            return _instance;
        }
    }
}