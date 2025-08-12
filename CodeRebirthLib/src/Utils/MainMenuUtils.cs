using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CodeRebirthLib;
public class MenuUtils
{
    //Thanks to LethalConfig and LethalPhones
    internal static void InjectMenu(Transform mainButtonsTransform, GameObject quitButton)
    {
        GameObject clonedButton = UnityEngine.Object.Instantiate(quitButton, mainButtonsTransform);
        Button clonedButtonComponent = clonedButton.GetComponent<Button>();

        clonedButtonComponent.onClick.RemoveAllListeners();
        clonedButtonComponent.onClick = new Button.ButtonClickedEvent();
        clonedButtonComponent.onClick.AddListener(AchievementUICanvas.Instance!.AchievementsButtonOnClick);
        clonedButtonComponent.onClick.AddListener(AchievementUICanvas.Instance._menuManager.PlayConfirmSFX);

        clonedButton.GetComponentInChildren<TextMeshProUGUI>().text = "> Achievements";
        AchievementUICanvas.Instance._achievementsButton = clonedButtonComponent;

        // wtf is going on down here
        List<GameObject> buttonsList = mainButtonsTransform.GetComponentsInChildren<Button>().Select(b => b.gameObject).ToList();
        List<float> positions = buttonsList.Where(b => b != clonedButton).Select(b => b.transform as RectTransform).Select(t => t!.anchoredPosition.y).ToList(); // TODO combine the two select's
        var offsets = positions.Zip(positions.Skip(1), (y1, y2) => Mathf.Abs(y2 - y1));
        float offset = offsets.Min();

        foreach (var button in buttonsList.Where(go => go != quitButton))
        {
            button.GetComponent<RectTransform>().anchoredPosition += new Vector2(0, offset);
        }

        clonedButton.GetComponent<RectTransform>().anchoredPosition = quitButton.GetComponent<RectTransform>().anchoredPosition + new Vector2(0, offset);
    }
}