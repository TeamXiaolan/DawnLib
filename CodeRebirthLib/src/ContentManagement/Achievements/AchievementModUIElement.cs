using System;
using System.IO;
using System.Linq;
using BepInEx.Bootstrap;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CodeRebirthLib.ContentManagement.Achievements;

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

    [SerializeField]
    private GameObject _modAchievementsContainerPrefab = null!;

    internal GameObject _achievementsContainer = null!;
    private GameObject _modAchievementsContainer = null!;
    internal void SetupModUI(CRMod mod)
    {
        _modAchievementsContainer = GameObject.Instantiate(_modAchievementsContainerPrefab, _achievementsContainer.transform);
        _modAchievementsContainer.name = $"CodeRebirthLib Achievement UI - {mod.Plugin.GUID}";

        _modNameText.text = mod.ModInformation.ModName;
        if (mod.ModInformation.ModIcon != null)
        {
            _modIcon.sprite = mod.ModInformation.ModIcon;
            _modIcon.color = Color.white;
        }

        foreach (var achievement in mod.AchievementRegistry())
        {
            CodeRebirthLibPlugin.ExtendedLogging($"Adding achievement: {achievement.AchievementName}");
            var uiElement = GameObject.Instantiate(_achievementUIElementPrefab, _modAchievementsContainer.transform);
            uiElement.GetComponent<AchievementUIElement>().SetupAchievementUI(achievement);
        }
        _achievementAccessButton.onClick.AddListener(OnButtonClick);
        _modAchievementsContainer.SetActive(false);
    }

    public void OnButtonClick()
    {
        // loop through all moduielements and disable all of em
        foreach (var modUIElement in AchievementUICanvas.Instance._modUIElements)
        {
            if (modUIElement == this)
                continue;

            modUIElement._modAchievementsContainer.SetActive(false);
        }
        _modAchievementsContainer.SetActive(!_modAchievementsContainer.activeSelf);
    }
}