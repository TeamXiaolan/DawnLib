using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CodeRebirthLib;

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
    private List<GameObject> _achievementsContainerList = new();

    internal void SetupModUI(CRMod mod)
    {
        _modNameText.text = mod.ModInformation.ModName;
        if (mod.ModInformation.ModIcon != null)
        {
            _modIcon.sprite = mod.ModInformation.ModIcon;
            _modIcon.color = Color.white;
        }

        var sortedAchievements = mod.AchievementRegistry().ToList()
            .Where(it => it.IsActive())
            .OrderByDescending(a => a.AchievementName)
            .ToList();

        foreach (var achievement in sortedAchievements)
        {
            Debuggers.ReplaceThis?.Log($"Adding achievement: {achievement.AchievementName}");

            var go = GameObject.Instantiate(_achievementUIElementPrefab, _achievementsContainer.transform);
            go.SetActive(false);

            var ui = go.GetComponent<AchievementUIElement>();
            ui.SetupAchievementUI(achievement);

            go.name = $"CodeRebirthLib Achievement UI - {achievement.AchievementName} - {mod.Plugin.GUID}";
            _achievementsContainerList.Add(go);
        }

        _achievementAccessButton.onClick.AddListener(OnButtonClick);
    }

    public void OnButtonClick()
    {
        // loop through all moduielements and disable all of em
        foreach (var modUIElement in AchievementUICanvas.Instance._modUIElements)
        {
            if (modUIElement == this)
                continue;

            foreach (var achievement in modUIElement._achievementsContainerList)
            {
                achievement.SetActive(!achievement.activeSelf);
            }
        }

        foreach (var achievement in _achievementsContainerList)
        {
            achievement.SetActive(!achievement.activeSelf);
        }
    }
}