
using System;
using CodeRebirthLib.Internal;
using CodeRebirthLib.Utils;
using TMPro;
using UnityEngine;

namespace CodeRebirthLib.CRMod;

static class CRAchievementHandler
{
    internal static ES3Settings globalSettings = new($"CRLib.Achievements", ES3.EncryptionType.None);
    public static event Action<CRMAchievementDefinition> OnAchievementUnlocked;

    internal static void LoadAll()
    {
        foreach (CRMAchievementDefinition achievementDefinition in CRModContent.Achievements.Values)
        {
            achievementDefinition.LoadAchievementState(globalSettings);
        }
    }

    internal static void SaveAll()
    {
        foreach (CRMAchievementDefinition achievementDefinition in CRModContent.Achievements.Values)
        {
            achievementDefinition.SaveAchievementState(globalSettings);
        }
    }

    internal static void UpdateUIElement(AchievementUIElement achievementUIElement, CRMAchievementDefinition achievementDefinition)
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