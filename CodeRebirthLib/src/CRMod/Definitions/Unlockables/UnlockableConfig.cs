using BepInEx.Configuration;

namespace CodeRebirthLib;

public class UnlockableConfig : EntityConfig
{
    public ConfigEntry<int> Cost;
    public ConfigEntry<bool> IsDecor;
    public ConfigEntry<bool>? IsProgressive = null;
    public ConfigEntry<bool> IsShipUpgrade;
}