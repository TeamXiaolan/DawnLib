namespace Dawn.Internal;

public static class QuickMenuManagerRefs
{
    private static QuickMenuManager _instance;
    public static QuickMenuManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = UnityEngine.Object.FindFirstObjectByType<QuickMenuManager>();
            }
            return _instance;
        }
    }
}