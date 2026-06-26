using UnityEngine;

namespace Dawn;

public class EnteringAtmosphereLoadingContext : ILoadingContext
{
    public void SetColor(Color color)
    {

    }

    public void SetText(string text)
    {
        if (HUDManager.Instance == null)
        {
            return;
        }

        HUDManager.Instance.loadingText.text = text;
    }
}