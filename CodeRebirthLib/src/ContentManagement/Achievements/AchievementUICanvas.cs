using System.Collections.Generic;
using System.Linq;
using CodeRebirthLib.Util;
using UnityEngine;
using UnityEngine.UI;

namespace CodeRebirthLib.ContentManagement.Achievements;

public class AchievementUICanvas : Singleton<AchievementUICanvas>
{
    [SerializeField]
    internal GameObject _achievementContents = null!;

    [SerializeField]
    private GameObject _modContents = null!;

    [SerializeField]
    private Button _backButton = null!;

    [SerializeField]
    private GameObject _background = null!;

    [SerializeField]
    private GameObject _openAchievementsButtonPrefab = null!;

    [SerializeField]
    private GameObject _achievementModUIElementPrefab = null!;

    private Button _achievementsButton = null!;
    private GameObject _mainButtons = null!;
    internal MenuManager _menuManager = null!;
    internal List<AchievementModUIElement> _modUIElements = new();

    private void Start()
    {
        _mainButtons = _menuManager.menuButtons;
        var openAchievementsButton = GameObject.Instantiate(_openAchievementsButtonPrefab, _mainButtons.transform);
        _achievementsButton = openAchievementsButton.GetComponent<Button>();

        _achievementsButton.onClick.AddListener(AchievementsButtonOnClick);
        _backButton.onClick.AddListener(BackButtonOnClick);
        _backButton.onClick.AddListener(_menuManager.PlayCancelSFX);
        _achievementsButton.onClick.AddListener(_menuManager.PlayConfirmSFX);
        AddAllAchievementsToContents();
    }

    private void AddAllAchievementsToContents()
    {
        foreach (var crMod in CRMod.AllMods)
        {
            // instantiate a mod element if it has ANY achievements
            if (crMod.AchievementRegistry().Count() > 0)
            {
                var uiElement = GameObject.Instantiate(_achievementModUIElementPrefab, _modContents.transform);
                AchievementModUIElement modUIElement = uiElement.GetComponent<AchievementModUIElement>();
                modUIElement._achievementsContainer = _achievementContents;
                _modUIElements.Add(modUIElement);
                modUIElement.SetupModUI(crMod);
            }
        }

    }

    public void BackButtonOnClick()
    {
        _menuManager.EnableUIPanel(_mainButtons);
        _achievementsButton.Select();
        _background.SetActive(false);
    }

    public void AchievementsButtonOnClick()
    {
        _mainButtons.SetActive(false);
        _background.SetActive(true);
        _backButton.Select();
    }
}