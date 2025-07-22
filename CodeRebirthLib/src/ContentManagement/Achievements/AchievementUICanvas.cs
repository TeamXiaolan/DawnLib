using UnityEngine;

namespace CodeRebirthLib.ContentManagement.Achievements;

public class AchievementUICanvas : MonoBehaviour
{
    [SerializeField]
    private GameObject _achievementContents = null!;

    public void Start()
    {
        AddAllAchievementsToContents();
    }

    private void AddAllAchievementsToContents()
    {
        foreach (var achievement in CRMod.AllAchievements())
        {
            CodeRebirthLibPlugin.ExtendedLogging($"Adding achievement: {achievement.AchievementName}");
            var uiElement = GameObject.Instantiate(CodeRebirthLibPlugin.Main.AchievementUIElementPrefab, _achievementContents.transform);
            uiElement.GetComponent<AchievementUIElement>().SetupAchievementUI(achievement);
        }
    }
}