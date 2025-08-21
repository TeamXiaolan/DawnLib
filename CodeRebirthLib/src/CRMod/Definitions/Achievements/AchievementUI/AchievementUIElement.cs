using CodeRebirthLib.Internal;
using CodeRebirthLib.Utils;
using TMPro;
using UnityEngine;
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

    public void SetupAchievementUI(CRMAchievementDefinition definition)
    {
        _achievementNameTMP.text = definition.AchievementName;
        _achievementDescriptionTMP.text = definition.AchievementDescription;
        _achievementIcon.sprite = definition.AchievementIcon;
        if (!definition.IsHidden)
        {
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
        }
        else
        {
            _achievementProgressGO.SetActive(false);
        }
    }
}