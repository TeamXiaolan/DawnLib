using System.Collections.Generic;
using System.Linq;
using Dawn.Internal;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Dawn.Dusk;

public class AchievementModUIElement : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI _modNameText = null!;

    [SerializeField]
    private Image _modIcon = null!;

    [SerializeField]
    private Button _achievementAccessButton = null!;

    [SerializeField]
    private GameObject _achievementUIElementPrefab = null!;

    internal GameObject _achievementsContainer = null!;

    internal List<AchievementUIElement> achievementsContainerList = new();
    internal static List<AchievementModUIElement> achievementModUIElements = new();

    private void Awake()
    {
        achievementModUIElements.Add(this);
    }

    private void OnDestroy()
    {
        achievementModUIElements.Remove(this);
    }

    internal void SetupModUI(DuskMod mod)
    {
        _modNameText.text = mod.ModInformation.ModName;
        if (mod.ModInformation.ModIcon != null)
        {
            _modIcon.sprite = mod.ModInformation.ModIcon;
            _modIcon.color = Color.white;
        }

        List<DuskAchievementDefinition> sortedAchievements = DuskModContent.Achievements.Values
            .OrderByDescending(a => a.AchievementName)
            .ToList();

        foreach (DuskAchievementDefinition achievement in sortedAchievements)
        {
            Debuggers.Achievements?.Log($"Adding achievement: {achievement.AchievementName}");

            GameObject go = GameObject.Instantiate(_achievementUIElementPrefab, _achievementsContainer.transform);
            go.SetActive(false);

            AchievementUIElement uiElement = go.GetComponent<AchievementUIElement>();
            uiElement.SetupAchievementUI(achievement);

            go.name = $"CodeRebirthLib Achievement UI - {achievement.AchievementName} - {mod.Plugin.GUID}";
            achievementsContainerList.Add(uiElement);
        }

        _achievementAccessButton.onClick.AddListener(OnButtonClick);
    }

    public void OnButtonClick()
    {
        // loop through all moduielements and disable all of em
        foreach (var modUIElement in AchievementUICanvas.Instance!._modUIElements)
        {
            if (modUIElement == this)
                continue;

            foreach (var achievement in modUIElement.achievementsContainerList)
            {
                achievement.gameObject.SetActive(!achievement.gameObject.activeSelf);
            }
        }

        foreach (var achievement in achievementsContainerList)
        {
            achievement.gameObject.SetActive(!achievement.gameObject.activeSelf);
        }
    }
}