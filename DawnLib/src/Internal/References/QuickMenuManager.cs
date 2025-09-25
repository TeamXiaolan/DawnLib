using UnityEngine.SceneManagement;

namespace Dawn.Internal;

static class QuickMenuManagerRefs
{
    static QuickMenuManagerRefs()
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