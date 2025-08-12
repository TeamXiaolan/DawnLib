using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CodeRebirthLib.CRMod.AchievementUI;

public class AchievementUIGetCanvas : Singleton<AchievementUIGetCanvas>
{
    private static readonly int SlideOut = Animator.StringToHash("SlideOut");

    [SerializeField]
    private GameObject _achievementGetUIElementPrefab = null!;

    [SerializeField]
    private GameObject achievementContent = null!;

    private Queue<CRAchievementDefinition> achievementQueue = new();

    private void Start()
    {
        CRMod.OnAchievementUnlocked += QueuePopup;
    }

    private void OnDestroy()
    {
        CRMod.OnAchievementUnlocked -= QueuePopup;
    }

    internal void QueuePopup(CRAchievementDefinition achievement)
    {
        achievementQueue.Enqueue(achievement);
        if (achievementContent.transform.childCount == 0)
            StartCoroutine(ShowNextAchievement());
    }

    private IEnumerator ShowNextAchievement()
    {
        while (achievementQueue.Count > 0)
        {
            var achievement = achievementQueue.Dequeue();
            GameObject achievementElement = Instantiate(_achievementGetUIElementPrefab, achievementContent.transform);
            achievementElement.GetComponent<AchievementUIElement>().SetupAchievementUI(achievement);
            yield return new WaitForSeconds(achievement.PopupTime);
            achievementElement.GetComponent<Animator>().SetTrigger(SlideOut);
            yield return new WaitForSeconds(3f);
            Destroy(achievementElement);
        }
    }
}