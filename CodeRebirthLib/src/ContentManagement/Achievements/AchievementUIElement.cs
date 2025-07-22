using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CodeRebirthLib.ContentManagement.Achievements;

public class AchievementUIElement : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI _achievementNameTMP = null!;

    [SerializeField]
    private TextMeshProUGUI _achievementDescriptionTMP = null!;

    [SerializeField]
    private GameObject _achievementProgressGO = null!;

    [SerializeField]
    private Image _achievementIcon = null!;

    public void SetupAchievementUI(CRAchievementBaseDefinition definition)
    {
        _achievementNameTMP.text = definition.AchievementName;
        _achievementDescriptionTMP.text = definition.AchievementDescription;
        _achievementIcon.sprite = definition.AchievementIcon;
        if (definition is CRProgressiveAchievement progressiveAchievement)
        {
            Image image = _achievementProgressGO.GetComponentInChildren<Image>();
            image.fillAmount = progressiveAchievement.Percentage;
        }
        else if (definition is CRInstantAchievement instantAchievement)
        {
            _achievementProgressGO.SetActive(false);
        }
    }
}