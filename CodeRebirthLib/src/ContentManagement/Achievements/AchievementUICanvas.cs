using UnityEngine;
using UnityEngine.UI;

namespace CodeRebirthLib.ContentManagement.Achievements;

public class AchievementUICanvas : MonoBehaviour
{
    [SerializeField]
    private GameObject _achievementContents = null!;

    [SerializeField]
    private Button _backButton = null!;
    [SerializeField]
    private GameObject _background = null!;

    [SerializeField]
    private GameObject _openAchievementsButtonPrefab = null!;

    private Button _achievementsButton = null!;
    private GameObject _mainButtons = null!;
    internal MenuManager _menuManager = null!;

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
        foreach (var achievement in CRMod.AllAchievements())
        {
            CodeRebirthLibPlugin.ExtendedLogging($"Adding achievement: {achievement.AchievementName}");
            var uiElement = GameObject.Instantiate(CodeRebirthLibPlugin.Main.AchievementUIElementPrefab, _achievementContents.transform);
            uiElement.GetComponent<AchievementUIElement>().SetupAchievementUI(achievement);
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