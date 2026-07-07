using EasyTextEffects;
using EasyTextEffects.Effects;
using TMPro;
using UnityEngine.UI;

namespace Dawn.Internal;

public static class HUDManagerRefs
{
    private static HUDManager _instance;
    public static HUDManager Instance
    {
        get
        {
            if (_instance == null)
            {
                if (HUDManager.Instance != null)
                {
                    _instance = HUDManager.Instance;
                }
                else
                {
                    _instance = UnityEngine.Object.FindFirstObjectByType<HUDManager>();
                }

                if (_instance == null)
                {
#pragma warning disable CS8603 // Possible null reference return.
                    return null;
#pragma warning restore CS8603 // Possible null reference return.
                }

                MainLoadingBackground = _instance.LoadingScreen.transform.Find("TextBG").GetComponent<Image>();
                MainLoadingText = _instance.LoadingScreen.transform.Find("LoadText").GetComponent<TextMeshProUGUI>();
                MainTextEffectColor = (Effect_Color)((Effect_Composite)MainLoadingTextEffect.globalEffects[0].effect).effects[0];
                SecondaryLoadingText = _instance.loadingText;
            }
            return _instance;
        }
    }

    public static Image MainLoadingBackground { get; private set; }
    public static TextMeshProUGUI MainLoadingText { get; private set; }
    public static TextEffect MainLoadingTextEffect { get; private set; }
    public static Effect_Color MainTextEffectColor { get; private set; }
    public static TextMeshProUGUI SecondaryLoadingText { get; private set; }
}