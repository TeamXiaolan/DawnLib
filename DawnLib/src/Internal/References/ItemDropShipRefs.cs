namespace Dawn.Internal;

public static class ItemDropshipRefs
{
    private static ItemDropship _instance;
    public static ItemDropship Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = UnityEngine.Object.FindFirstObjectByType<ItemDropship>();
            }
            return _instance;
        }
    }
}