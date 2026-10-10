using System;
using Dawn.Utils;
using TMPro;
using UnityEngine;
using Dawn.Internal;

namespace Eventide;

static class EventideAchievementHandler
{
    public static Action<EventideAchievementDefinition>? OnAchievementUnlocked;

    internal static void LoadAll()
    {
        using (EventidePlugin.PersistentData.CreateEditContext())
        {
            foreach (EventideAchievementDefinition achievementDefinition in EventideModContent.Achievements)
            {
                achievementDefinition.LoadAchievementState(EventidePlugin.PersistentData);
            }
        }
    }

    internal static void SaveAll()
    {
        using (EventidePlugin.PersistentData.CreateEditContext())
        {
            foreach (EventideAchievementDefinition achievementDefinition in EventideModContent.Achievements)
            {
                achievementDefinition.SaveAchievementState(EventidePlugin.PersistentData);
            }
        }
    }

    internal static void UpdateUIElement(AchievementUIElement achievementUIElement, EventideAchievementDefinition achievementDefinition)
    {
        achievementUIElement.achievementNameTMP.text = achievementDefinition.AchievementName;
        achievementUIElement.achievementDescriptionTMP.text = achievementDefinition.AchievementDescription;
        achievementUIElement.achievementIcon.sprite = achievementDefinition.AchievementIcon;
        achievementUIElement.achievementHiddenButton.interactable = false;
        if (achievementUIElement.eventTrigger != null)
        {
            achievementUIElement.eventTrigger.enabled = false;
        }

        if (achievementDefinition.CanBeUnhidden)
        {
            achievementUIElement.achievementHiddenButton.interactable = true;
            if (achievementUIElement.eventTrigger != null)
            {
                achievementUIElement.eventTrigger.enabled = true;
            }
        }

        if (!achievementDefinition.IsHidden || achievementDefinition.Completed)
        {
            if (achievementUIElement.eventTrigger != null)
            {
                achievementUIElement.eventTrigger.enabled = true;
            }
            achievementUIElement.achievementHiddenButton.interactable = true;
            achievementUIElement.achievementHiddenButton.onClick.Invoke();
        }

        if (!achievementDefinition.Completed)
        {
            achievementUIElement.achievementNameTMP.color = achievementUIElement.unfinishedAchievementColor;
            achievementUIElement.achievementDescriptionTMP.color = achievementUIElement.unfinishedAchievementColor;
            achievementUIElement.backgroundImage.sprite = null;
            achievementUIElement.backgroundImage.color = new Color32(0, 0, 0, 107);
            achievementUIElement.achievementNameTMP.colorGradientPreset = null;
            achievementUIElement.achievementDescriptionTMP.colorGradientPreset = null; ;
        }
        else
        {
            if (achievementDefinition.FinishedAchievementBackgroundIcon != null)
            {
                achievementUIElement.backgroundImage.sprite = achievementDefinition.FinishedAchievementBackgroundIcon;
                achievementUIElement.backgroundImage.color = Color.white;
            }
            achievementUIElement.achievementNameTMP.colorGradientPreset = achievementDefinition.FinishedAchievementNameColorGradientPreset;
            achievementUIElement.achievementDescriptionTMP.colorGradientPreset = achievementDefinition.FinishedAchievementDescColorGradientPreset;
        }

        if (achievementDefinition is IProgress progressiveAchievement)
        {
            Debuggers.Achievements?.Log($"Setting up progress achievement: {achievementDefinition.AchievementName} with percentage: {progressiveAchievement.Percentage()}");
            achievementUIElement.progressBar.fillAmount = progressiveAchievement.Percentage();
            achievementUIElement.progressBar.GetComponentInChildren<TextMeshProUGUI>().text = $"{progressiveAchievement.CurrentProgress}/{progressiveAchievement.MaxProgress}";
        }
        else
        {
            achievementUIElement.achievementProgressGO.SetActive(false);
        }
    }
}