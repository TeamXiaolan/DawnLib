using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dusk;
public class ShipConfig
{
    public ConfigEntry<int> Cost;
    public ConfigEntry<bool>? DisableUnlockRequirements = null;
    public ConfigEntry<bool>? DisablePricingStrategy = null;
}
