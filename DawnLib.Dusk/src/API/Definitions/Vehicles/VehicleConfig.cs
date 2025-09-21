using BepInEx.Configuration;

namespace Dusk;
public class VehicleConfig
{
    public ConfigEntry<int> Cost;
    public ConfigEntry<bool>? DisableUnlockRequirements = null;
    public ConfigEntry<bool>? DisablePricingStrategy = null;
}