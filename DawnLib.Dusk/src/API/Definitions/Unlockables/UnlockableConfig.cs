using BepInEx.Configuration;

namespace Dusk;

public class UnlockableConfig(ConfigContext section, string EntityNameReference) : DuskBaseConfig(section, EntityNameReference)
{
    public ConfigEntry<int>? Cost;
    public ConfigEntry<bool>? IsDecor;
    public ConfigEntry<bool>? DisableUnlockRequirement;
    public ConfigEntry<bool>? DisablePricingStrategy;
    public ConfigEntry<bool>? IsShipUpgrade;
}