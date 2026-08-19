using DunGen.Graph;

namespace Dawn.Internal;

public static class RoundManagerRefs
{
    private static RoundManager _instance;
    public static RoundManager Instance
    {
        get
        {
            if (_instance == null)
            {
                if (RoundManager.Instance != null)
                {
                    _instance = RoundManager.Instance;
                }
                else
                {
                    _instance = UnityEngine.Object.FindFirstObjectByType<RoundManager>();
                }
            }
            return _instance;
        }
    }

    public static DungeonFlow? GetCurrentDungeon()
    {
        if (Instance == null)
        {
            return null;
        }

        if (Instance.dungeonGenerator == null)
        {
            return null;
        }

        if (Instance.dungeonGenerator.Generator == null)
        {
            return null;
        }

        return Instance.dungeonGenerator.Generator.DungeonFlow;
    }

    public static DawnDungeonInfo? GetCurrentDungeonInfo()
    {
        DungeonFlow? dungeonFlow = GetCurrentDungeon();
        if (dungeonFlow == null)
        {
            return null;
        }
        return dungeonFlow.DawnInfo;
    }
}