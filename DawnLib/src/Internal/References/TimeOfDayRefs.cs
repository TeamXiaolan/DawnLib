
using UnityEngine.SceneManagement;

namespace Dawn.Internal;

internal static class TimeOfDayRefs
{
    static TimeOfDayRefs()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
    {
        if (scene.name == "SampleSceneRelay")
        {
            _ = Instance;
        }
    }

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