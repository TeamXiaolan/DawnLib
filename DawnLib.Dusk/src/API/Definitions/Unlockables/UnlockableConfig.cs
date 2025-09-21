using BepInEx.Configuration;

namespace Dusk;

public class UnlockableConfig
{
    public ConfigEntry<int> Cost;
    public ConfigEntry<bool> IsDecor;
    public ConfigEntry<bool>? DisableUnlockRequirement;
    public ConfigEntry<bool>? DisablePricingStrategy;
    public ConfigEntry<bool> IsShipUpgrade;
}