using CodeRebirthLib.Internal;
using CodeRebirthLib.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CodeRebirthLib.CRMod;

public class AchievementUIElement : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI _achievementNameTMP = null!;

    [SerializeField]
    private TextMeshProUGUI _achievementDescriptionTMP = null!;

    [SerializeField]
    private GameObject _achievementProgressGO = null!;

    [SerializeField]
    private Button _achievementHiddenButton = null!;

    [SerializeField]
    private Image _achievementIcon = null!;

    [SerializeField]
    private Color _unfinishedAchievementColor = new(0.5f, 0.5f, 0.5f);

    [SerializeField]
    private Image _backgroundImage = null!;

    [SerializeField]
    private Image _progressBar = null!;

    [SerializeField]
    private EventTrigger? _eventTrigger = null;

    public void SetupAchievementUI(CRMAchievementDefinition definition)
    {
        _achievementNameTMP.text = definition.AchievementName;
        _achievementDescriptionTMP.text = definition.AchievementDescription;
        _achievementIcon.sprite = definition.AchievementIcon;
        _achievementHiddenButton.interactable = false;
        if (_eventTrigger != null)
        {
            _eventTrigger.enabled = false;
        }

        if (definition.CanBeUnhidden)
        {
            _achievementHiddenButton.interactable = true;
            if (_eventTrigger != null)
            {
                _eventTrigger.enabled = true;
            }
        }

        if (!definition.IsHidden || definition.Completed)
        {
            if (_eventTrigger != null)
            {
                _eventTrigger.enabled = true;
            }
            _achievementHiddenButton.interactable = true;
            _achievementHiddenButton.onClick.Invoke();
        }

        if (!definition.Completed)
        {
            _achievementNameTMP.color = _unfinishedAchievementColor;
            _achievementDescriptionTMP.color = _unfinishedAchievementColor;
        }
        else
        {
            if (definition.FinishedAchievementBackgroundIcon != null)
            {
                _backgroundImage.sprite = definition.FinishedAchievementBackgroundIcon;
                _backgroundImage.color = Color.white;
            }
            _achievementNameTMP.colorGradientPreset = definition.FinishedAchievementNameColorGradientPreset;
            _achievementDescriptionTMP.colorGradientPreset = definition.FinishedAchievementDescColorGradientPreset;
        }

        if (definition is IProgress progressiveAchievement)
        {
            Debuggers.Achievements?.Log($"Setting up progress achievement: {definition.AchievementName} with percentage: {progressiveAchievement.Percentage()}");
            _progressBar.fillAmount = progressiveAchievement.Percentage();
            _progressBar.GetComponentInChildren<TextMeshProUGUI>().text = $"{progressiveAchievement.CurrentProgress}/{progressiveAchievement.MaxProgress}";
        }
        else
        {
            _achievementProgressGO.SetActive(false);
        }
    }
}