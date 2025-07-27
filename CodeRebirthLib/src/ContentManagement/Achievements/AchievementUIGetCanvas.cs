using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CodeRebirthLib.Util;
using UnityEngine;
using UnityEngine.UI;

namespace CodeRebirthLib.ContentManagement.Achievements;

public class AchievementUIGetCanvas : Singleton<AchievementUIGetCanvas>
{
    private static readonly int SlideOut = Animator.StringToHash("SlideOut");
    
    [SerializeField]
    private GameObject _achievementGetUIElementPrefab = null!;
    
    [SerializeField]
    private GameObject achievementContent = null!;

    [SerializeField]
    private float PopupTime = 2f;
    
    private Queue<CRAchievementBaseDefinition> achievementQueue = new();

    private void Start()
    {
        CRMod.OnAchievementUnlocked += QueuePopup;
    }

    private void OnDestroy()
    {
        CRMod.OnAchievementUnlocked -= QueuePopup;
    }

    private void QueuePopup(CRAchievementBaseDefinition achievement)
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
            GameObject achivementElement = Instantiate(_achievementGetUIElementPrefab, achievementContent.transform);
            achivementElement.GetComponent<AchievementUIElement>().SetupAchievementUI(achievement);
            yield return new WaitForSeconds(PopupTime);
            achivementElement.GetComponent<Animator>().SetTrigger(SlideOut);
            yield return new WaitForSeconds(PopupTime);
            Destroy(achivementElement);
        }
    }
}