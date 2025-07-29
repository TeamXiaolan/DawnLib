using System.Linq;
using CodeRebirthLib.ContentManagement.Achievements;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CodeRebirthLib.Util;
public class MenuUtils
{
    //Thanks to LethalConfig and LethalPhones
    internal static void InjectMenu(Transform mainButtonsTransform, GameObject quitButton)
    {
        var clonedButton = UnityEngine.Object.Instantiate(quitButton, mainButtonsTransform);
        clonedButton.GetComponent<Button>().onClick.RemoveAllListeners();
        clonedButton.GetComponent<Button>().onClick = new Button.ButtonClickedEvent();
        clonedButton.GetComponent<Button>().onClick.AddListener(AchievementUICanvas.Instance!.AchievementsButtonOnClick);
        clonedButton.GetComponent<Button>().onClick.AddListener(AchievementUICanvas.Instance._menuManager.PlayConfirmSFX);
        clonedButton.GetComponentInChildren<TextMeshProUGUI>().text = "> Achievements";
        AchievementUICanvas.Instance._achievementsButton = clonedButton.GetComponent<Button>();
        
        var buttonsList = mainButtonsTransform.GetComponentsInChildren<Button>()
            .Select(b => b.gameObject);
        
        var gameObjects = buttonsList.ToList();
        var positions = gameObjects
            .Where(b => b != clonedButton)
            .Select(b => b.transform as RectTransform)
            .Select(t => t!.anchoredPosition.y);
        var enumerable = positions.ToList();
        var offsets = enumerable
            .Zip(enumerable.Skip(1), (y1, y2) => Mathf.Abs(y2 - y1));
        var offset = offsets.Min();

        foreach (var button in gameObjects.Where(g => g != quitButton))
            button.GetComponent<RectTransform>().anchoredPosition += new Vector2(0, offset);

        clonedButton.GetComponent<RectTransform>().anchoredPosition =
            quitButton.GetComponent<RectTransform>().anchoredPosition + new Vector2(0, offset);

    }
}