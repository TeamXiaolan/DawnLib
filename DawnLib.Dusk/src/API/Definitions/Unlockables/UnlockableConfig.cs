using System.Collections.Generic;
using BepInEx.Configuration;

namespace Dusk;

public class UnlockableConfig(ConfigContext section, string EntityNameReference) : DuskBaseConfig(section, EntityNameReference)
{
    public ConfigEntry<int>? Cost;

    public ConfigEntry<bool>? IsDecor;
    public ConfigEntry<bool>? IsShipUpgrade;

    public ConfigEntry<bool>? DisableUnlockRequirement;
    public ConfigEntry<bool>? DisablePricingStrategy;

    override internal List<ConfigEntryBase?> _configEntries => [
        Cost,

        IsShipUpgrade,
        IsDecor,

        DisableUnlockRequirement,
        DisablePricingStrategy
    ];
}