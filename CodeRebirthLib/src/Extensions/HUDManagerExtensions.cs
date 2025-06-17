using CodeRebirthLib.Util;

namespace CodeRebirthLib.Extensions;
public static class HUDManagerExtensions
{
    public static void DisplayTip(this HUDManager instance, HUDDisplayTip displayTip)
    {
        instance.DisplayTip(displayTip.Header, displayTip.Body);
    }
}