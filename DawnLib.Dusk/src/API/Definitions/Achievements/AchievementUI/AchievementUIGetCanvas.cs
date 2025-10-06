using System.Collections;
using System.Collections.Generic;
using Dawn.Utils;
using UnityEngine;

namespace Dusk;

public class AchievementUIGetCanvas : Singleton<AchievementUIGetCanvas>
{
    private static readonly int SlideOut = Animator.StringToHash("SlideOut");

    [SerializeField]
    private GameObject _achievementGetUIElementPrefab = null!;

    [SerializeField]
    private GameObject achievementContent = null!;

    private Queue<DuskAchievementDefinition> achievementQueue = new();

    private void Start()
    {
        DuskAchievementHandler.OnAchievementUnlocked += QueuePopup;
    }

    private void OnDestroy()
    {
        DuskAchievementHandler.OnAchievementUnlocked -= QueuePopup;
    }

    internal void QueuePopup(DuskAchievementDefinition achievement)
    {
        achievementQueue.Enqueue(achievement);
        if (achievementContent.transform.childCount == 0)
        {
            StartCoroutine(ShowNextAchievement());
        }
    }

    private IEnumerator ShowNextAchievement()
    {
        while (achievementQueue.Count > 0)
        {
            DuskAchievementDefinition achievementDefinition = achievementQueue.Dequeue();
            GameObject achievementElement = Instantiate(_achievementGetUIElementPrefab, achievementContent.transform);
            AchievementUIElement uiElement = achievementElement.GetComponent<AchievementUIElement>();
            uiElement.SetupAchievementUI(achievementDefinition);
            if (achievementDefinition.FinishAchievementAudioClip != null)
            {
                uiElement.audioSource.clip = achievementDefinition.FinishAchievementAudioClip;
            }
            uiElement.audioSource.Play();
            yield return new WaitForSeconds(achievementDefinition.PopupTime);
            achievementElement.GetComponent<Animator>().SetTrigger(SlideOut);
            yield return new WaitForSeconds(3f);
            Destroy(achievementElement);
        }
    }
}