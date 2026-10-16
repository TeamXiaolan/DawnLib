using System;
using System.Collections.Generic;
using System.Globalization;
using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using Dawn;
using Dawn.Internal;
using Dawn.Utils;
using UnityEngine;
using UnityEngine.Events;

namespace Sundown.Utils;

public static class ConfigReaderTypeUtility
{
    private static readonly Dictionary<Type, SundownDynamicConfigType> SettingTypeToDynamicType = new()
    {
        { typeof(string), SundownDynamicConfigType.String },
        { typeof(int), SundownDynamicConfigType.Int },
        { typeof(float), SundownDynamicConfigType.Float },
        { typeof(bool), SundownDynamicConfigType.Bool },
        { typeof(BoundedRange), SundownDynamicConfigType.BoundedRange },
        { typeof(Vector3), SundownDynamicConfigType.Vector3 },
        { typeof(Color), SundownDynamicConfigType.Color },
        { typeof(AnimationCurve), SundownDynamicConfigType.AnimationCurve },
    };

    private static readonly Dictionary<SundownDynamicConfigType, string> DynamicTypeToEventFieldName = new()
    {
        { SundownDynamicConfigType.String, "onString" },
        { SundownDynamicConfigType.Int, "onInt" },
        { SundownDynamicConfigType.Float, "onFloat" },
        { SundownDynamicConfigType.Bool, "onBool" },
        { SundownDynamicConfigType.BoundedRange, "onBoundedRange" },
        { SundownDynamicConfigType.Vector3, "onVector3" },
        { SundownDynamicConfigType.Color, "onColor" },
        { SundownDynamicConfigType.AnimationCurve, "onAnimationCurve" },
    };

    public static SundownDynamicConfigType? ConvertSettingTypeToDynamicType(Type settingType)
    {
        if (SettingTypeToDynamicType.TryGetValue(settingType, out SundownDynamicConfigType dynamicType))
        {
            return dynamicType;
        }

        return null;
    }

    public static string GetEventFieldName(SundownDynamicConfigType configType)
    {
        if (DynamicTypeToEventFieldName.TryGetValue(configType, out string fieldName))
        {
            return fieldName;
        }

        throw new ArgumentOutOfRangeException(nameof(configType), configType, null);
    }
}

[AddComponentMenu($"{DawnConstants.MiscUtils}/Config Reader")]
public class ConfigReader : MonoBehaviour
{
    [Header("Config Target")]
    [SerializeField] private string pluginGuid = "";
    [SerializeField] private string section = "";
    [SerializeField] private string key = "";
    [SerializeField] private SundownDynamicConfigType expectedType;

    [Header("Invoke")]
    [SerializeField] private bool invokeOnStart = true;

    [Header("Events")]
    [SerializeField] private UnityEvent onUnsupportedType;
    [SerializeField] private UnityEvent onEntryNotFound;
    [SerializeField] private UnityEvent onTypeMismatch;

    [SerializeField] private StringEvent onString;
    [SerializeField] private FloatEvent onFloat;
    [SerializeField] private IntEvent onInt;
    [SerializeField] private BoolEvent onBool;
    [SerializeField] private BoundedRangeEvent onBoundedRange;
    [SerializeField] private Vector3Event onVector3;
    [SerializeField] private ColorEvent onColor;
    [SerializeField] private AnimationCurveEvent onAnimationCurve;

    private object? _matchedEntry;

    public SundownDynamicConfigType ExpectedType => expectedType;
    public object? MatchedEntry => _matchedEntry;

    private void Start()
    {
        ResolveEntry();

        if (invokeOnStart)
        {
            InvokeMatchedEvent();
        }
    }

    public void ResolveEntry()
    {
        _matchedEntry = TryFindConfigEntry(pluginGuid, section, key);
        if (_matchedEntry == null)
        {
            SundownPlugin.Logger.LogWarning($"ConfigReader could not find config entry: plugin='{pluginGuid}', section='{section}', key='{key}'");
            return;
        }

        ConfigEntryBase configEntry = (ConfigEntryBase)_matchedEntry;

        Debuggers.Configs?.Log($"Matched config entry: plugin='{pluginGuid}', section='{configEntry.Definition.Section}', key='{configEntry.Definition.Key}', type='{configEntry.SettingType}', value='{configEntry.BoxedValue}'");
    }

    public void InvokeMatchedEvent()
    {
        if (_matchedEntry == null)
        {
            ResolveEntry();
        }

        if (_matchedEntry == null)
        {
            onEntryNotFound?.Invoke();
            return;
        }

        ConfigEntryBase configEntry = (ConfigEntryBase)_matchedEntry;
        SundownDynamicConfigType? actualType = ConfigReaderTypeUtility.ConvertSettingTypeToDynamicType(configEntry.SettingType);
        if (actualType == null)
        {
            SundownPlugin.Logger.LogWarning($"Unsupported config type on ConfigReader: {configEntry.SettingType.FullName}");
            onUnsupportedType?.Invoke();
            return;
        }

        if (actualType.Value != expectedType)
        {
            SundownPlugin.Logger.LogWarning($"ConfigReader type mismatch for '{pluginGuid}:{section}:{key}'. Expected '{expectedType}', but config entry was '{actualType.Value}'.");
            onTypeMismatch?.Invoke();
            return;
        }

        InvokeExpectedTypeEvent(configEntry.BoxedValue);
    }

    private void InvokeExpectedTypeEvent(object value)
    {
        switch (expectedType)
        {
            case SundownDynamicConfigType.String:
                onString?.Invoke((string)value);
                return;

            case SundownDynamicConfigType.Int:
                onInt?.Invoke((int)value);
                return;

            case SundownDynamicConfigType.Float:
                onFloat?.Invoke(Convert.ToSingle(value, CultureInfo.InvariantCulture));
                return;

            case SundownDynamicConfigType.Bool:
                onBool?.Invoke((bool)value);
                return;

            case SundownDynamicConfigType.BoundedRange:
                onBoundedRange?.Invoke((BoundedRange)value);
                return;

            case SundownDynamicConfigType.Vector3:
                onVector3?.Invoke((Vector3)value);
                return;

            case SundownDynamicConfigType.Color:
                onColor?.Invoke((Color)value);
                return;

            case SundownDynamicConfigType.AnimationCurve:
                onAnimationCurve?.Invoke((AnimationCurve)value);
                return;

            default:
                SundownPlugin.Logger.LogWarning($"Unhandled expected config type: {expectedType}");
                onUnsupportedType?.Invoke();
                return;
        }
    }

    private static object? TryFindConfigEntry(string pluginGuid, string section, string key)
    {
        if (string.IsNullOrWhiteSpace(pluginGuid) || string.IsNullOrWhiteSpace(section) || string.IsNullOrWhiteSpace(key))
        {
            return null;
        }

        if (!Chainloader.PluginInfos.TryGetValue(pluginGuid, out PluginInfo pluginInfo))
        {
            return null;
        }

        Debuggers.Configs?.Log($"ConfigReader: pluginGuid={pluginGuid}, section={section}, key={key}");
        ConfigFile config = pluginInfo.Instance.Config;
        ConfigDefinition definition = new(section, key);
        // Note: explicit cast needed due to https://github.com/BepInEx/BepInEx/issues/1305
        var entries = ((IDictionary<ConfigDefinition, ConfigEntryBase>)config).Values;
        foreach (ConfigEntryBase configEntryBase in entries)
        {
            ConfigDefinition existingDefinition = configEntryBase.Definition;
            Debuggers.Configs?.Log($"Existing definition: Section: {existingDefinition.Section} | Key: {existingDefinition.Key}");
            if (existingDefinition == definition)
            {
                return configEntryBase;
            }
        }

        return null;
    }
}