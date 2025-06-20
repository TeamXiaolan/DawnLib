using BepInEx.Configuration;
using CodeRebirthLib.ConfigManagement;

namespace CodeRebirthLib.ContentManagement.Unlockables;
public class UnlockableConfig : CRContentConfig
{
    public ConfigEntry<int> Cost;
    public ConfigEntry<bool> IsDecor;
    public ConfigEntry<bool>? IsProgressive = null;
    public ConfigEntry<bool> IsShipUpgrade;
}