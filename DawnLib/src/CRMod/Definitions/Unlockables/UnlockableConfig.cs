using BepInEx.Configuration;

namespace CodeRebirthLib.CRMod;

public class UnlockableConfig : EntityConfig
{
    public ConfigEntry<int> Cost;
    public ConfigEntry<bool> IsDecor;
    public ConfigEntry<bool>? DisableUnlockRequirement;
    public ConfigEntry<bool> IsShipUpgrade;
}