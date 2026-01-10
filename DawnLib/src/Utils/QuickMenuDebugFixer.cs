using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Dawn.Utils;
public class TMPDropDownFixer : MonoBehaviour
{
    private float normalizedScrollPosition = 0f;
    private ScrollRect? scrollRect = null;

    internal void OnOpenDropdown(TMP_Dropdown dropdown)
    {
        scrollRect = dropdown.m_Dropdown.GetComponent<ScrollRect>();
        scrollRect.SetNormalizedPosition(normalizedScrollPosition, 1);
    }

    internal void OnCloseDropdown()
    {
        if (scrollRect == null)
        {
            DawnPlugin.Logger.LogError($"Somehow closed dropdown without opening it???");
            return;
        }

        normalizedScrollPosition = scrollRect.normalizedPosition.y;
        scrollRect = null;
    }
}