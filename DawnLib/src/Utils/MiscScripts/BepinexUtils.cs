using System;
using BepInEx.Configuration;

namespace Dawn.Utils;

internal class BepinexUtils
{
    public static ConfigEntry<T> CreateConfigItem<T>(ConfigFile ModConfig, string section, string configItemName, T defaultValue, string ConfigDescription)
    {
        section = section.BepinFriendlyString();
        configItemName = configItemName.BepinFriendlyString();

        return ModConfig.Bind<T>(section, configItemName, defaultValue, ConfigDescription);
    }

    public static ConfigEntry<T> CreateConfigItem<T>(ConfigFile ModConfig, string section, string configItemName, T defaultValue, string description, AcceptableValueList<T> acceptableValues = null!) where T : IEquatable<T>
    {
        section = section.BepinFriendlyString();
        configItemName = configItemName.BepinFriendlyString();

        return ModConfig.Bind<T>(section, configItemName, defaultValue, new ConfigDescription(description, acceptableValues));
    }

    public static ConfigEntry<T> CreateConfigItem<T>(ConfigFile ModConfig, string section, string configItemName, T defaultValue, string description, T minValue, T maxValue) where T : IComparable
    {
        section = section.BepinFriendlyString();
        configItemName = configItemName.BepinFriendlyString();
        AcceptableValueRange<T> acceptableRange = new(minValue, maxValue);

        return ModConfig.Bind<T>(section, configItemName, defaultValue, new ConfigDescription(description, acceptableRange));
    }
}
