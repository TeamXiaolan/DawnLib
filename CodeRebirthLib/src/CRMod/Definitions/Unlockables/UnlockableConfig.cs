using BepInEx.Configuration;

namespace CodeRebirthLib.CRMod;

public class UnlockableConfig : EntityConfig
{
    public ConfigEntry<int> Cost;
    public ConfigEntry<bool> IsDecor;
    public ConfigEntry<bool>? IsProgressive;
    public ConfigEntry<bool> IsShipUpgrade;
}