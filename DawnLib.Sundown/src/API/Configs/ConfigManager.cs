using System;
using BepInEx.Configuration;
using Dusk;

namespace Sundown;
// todo: look over this again and see how much can become toml types?
public static class ConfigManager
{
    public static ConfigEntryBase CreateDynamicConfig(bool userAllowedToEdit, SundownDynamicConfig configDefinition, ConfigContext context)
    {
        ConfigEntryBase Bind<T>(T defaultValue)
        {
            ConfigEntry<T> configEntry = context.Bind(configDefinition.settingName, configDefinition.Description, defaultValue);
            if (!userAllowedToEdit)
            {
                DuskBaseConfig.AssignValueIfNotNull(configEntry, defaultValue);
            }
            return configEntry;
        }

        return configDefinition.DynamicConfigType switch
        {
            SundownDynamicConfigType.String => Bind(configDefinition.defaultString),
            SundownDynamicConfigType.Int => Bind(configDefinition.defaultInt),
            SundownDynamicConfigType.Bool => Bind(configDefinition.defaultBool),
            SundownDynamicConfigType.Float => Bind(configDefinition.defaultFloat),
            SundownDynamicConfigType.BoundedRange => Bind(configDefinition.defaultBoundedRange),
            SundownDynamicConfigType.AnimationCurve => Bind(configDefinition.defaultAnimationCurve),
            SundownDynamicConfigType.Vector3 => Bind(configDefinition.defaultVector3),
            SundownDynamicConfigType.Color => Bind(configDefinition.defaultColor),
            _ => throw new ArgumentOutOfRangeException($"DynamicConfigType of '{configDefinition.DynamicConfigType}' is not yet internally implemented!!"),
        };
    }
}