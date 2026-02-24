using System.Collections.Generic;
using BepInEx.Configuration;

namespace Dusk;
public class VehicleConfig(ConfigContext section, string EntityNameReference) : DuskBaseConfig(section, EntityNameReference)
{
    public ConfigEntry<int>? Cost;
    public ConfigEntry<bool>? DisableUnlockRequirements = null;
    public ConfigEntry<bool>? DisablePricingStrategy = null;

    override internal List<ConfigEntryBase?> _configEntries => [
        Cost,
        DisableUnlockRequirements,
        DisablePricingStrategy
    ];
}