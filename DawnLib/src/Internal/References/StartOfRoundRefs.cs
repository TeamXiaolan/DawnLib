using UnityEngine.SceneManagement;

namespace Dawn.Internal;

static class StartOfRoundRefs
{
    static StartOfRoundRefs()
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