using Dawn.Internal;
using UnityEngine;

namespace Dawn;

public class EnteringAtmosphereLoadingContext : ILoadingContext
{
    public void SetBackgroundColor(Color color)
    {
        if (HUDManagerRefs.Instance == null)
        {
            return;
        }

        HUDManagerRefs.MainLoadingBackground.color = color;
    }

    public void SetMainText(string text)
    {
        if (HUDManagerRefs.Instance == null)
        {
            return;
        }

        HUDManagerRefs.MainLoadingText.text = text;
    }

    public void SetMainTextColor(Color startColor, Color endColor)
    {
        if (HUDManagerRefs.Instance == null)
        {
            return;
        }

        HUDManagerRefs.MainTextEffectColor.startColor = startColor;
        HUDManagerRefs.MainTextEffectColor.endColor = endColor;
        HUDManagerRefs.MainLoadingTextEffect.Refresh();
    }

    public void SetSecondaryText(string text)
    {
        if (HUDManagerRefs.Instance == null)
        {
            return;
        }

        HUDManagerRefs.SecondaryLoadingText.text = text;
    }

    public void SetSecondaryTextColor(Color color)
    {
        if (HUDManagerRefs.Instance == null)
        {
            return;
        }

        HUDManagerRefs.SecondaryLoadingText.color = color;
    }
}