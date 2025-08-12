using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CodeRebirthLib.CRMod.AchievementUI;

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

    public void SetupAchievementUI(CRAchievementInfo definition)
    {
        _achievementNameTMP.text = definition.Name;
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
            Image image = _achievementProgressGO.GetComponentInChildren<Image>();
            image.fillAmount = progressiveAchievement.Percentage();
        }
        else
        {
            _achievementProgressGO.SetActive(false);
        }
    }
}