using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using Dawn.Internal;
using Dawn.Utils;
using UnityEngine;
using UnityEngine.Events;

namespace Dusk;

public static class ConfigReaderTypeUtility
{
    private static readonly Dictionary<Type, DuskDynamicConfigType> SettingTypeToDynamicType = new()
    {
        { typeof(string), DuskDynamicConfigType.String },
        { typeof(int), DuskDynamicConfigType.Int },
        { typeof(float), DuskDynamicConfigType.Float },
        { typeof(bool), DuskDynamicConfigType.Bool },
        { typeof(BoundedRange), DuskDynamicConfigType.BoundedRange },
        { typeof(Vector3), DuskDynamicConfigType.Vector3 },
        { typeof(Color), DuskDynamicConfigType.Color },
        { typeof(AnimationCurve), DuskDynamicConfigType.AnimationCurve },
    };

    private static readonly Dictionary<DuskDynamicConfigType, string> DynamicTypeToEventFieldName = new()
    {
        { DuskDynamicConfigType.String, "onString" },
        { DuskDynamicConfigType.Int, "onInt" },
        { DuskDynamicConfigType.Float, "onFloat" },
        { DuskDynamicConfigType.Bool, "onBool" },
        { DuskDynamicConfigType.BoundedRange, "onBoundedRange" },
        { DuskDynamicConfigType.Vector3, "onVector3" },
        { DuskDynamicConfigType.Color, "onColor" },
        { DuskDynamicConfigType.AnimationCurve, "onAnimationCurve" },
    };

    public static DuskDynamicConfigType? ConvertSettingTypeToDynamicType(Type settingType)
    {
        if (SettingTypeToDynamicType.TryGetValue(settingType, out DuskDynamicConfigType dynamicType))
        {
            return dynamicType;
        }

        return null;
    }

    public static string GetEventFieldName(DuskDynamicConfigType configType)
    {
        if (DynamicTypeToEventFieldName.TryGetValue(configType, out string fieldName))
        {
            return fieldName;
        }

        throw new ArgumentOutOfRangeException(nameof(configType), configType, null);
    }
}

public class ConfigReader : MonoBehaviour
{
    [Header("Config Target")]
    [SerializeField] private string pluginGuid = "";
    [SerializeField] private string section = "";
    [SerializeField] private string key = "";
    [SerializeField] private DuskDynamicConfigType expectedType;

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

    private ConfigEntryBase? _matchedEntry;

    public DuskDynamicConfigType ExpectedType => expectedType;
    public ConfigEntryBase? MatchedEntry => _matchedEntry;

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
            DuskPlugin.Logger.LogWarning($"ConfigReader could not find config entry: plugin='{pluginGuid}', section='{section}', key='{key}'");
            return;
        }

        Debuggers.Configs?.Log($"Matched config entry: plugin='{pluginGuid}', section='{_matchedEntry.Definition.Section}', key='{_matchedEntry.Definition.Key}', type='{_matchedEntry.SettingType}', value='{_matchedEntry.BoxedValue}'");
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

        DuskDynamicConfigType? actualType = ConfigReaderTypeUtility.ConvertSettingTypeToDynamicType(_matchedEntry.SettingType);
        if (actualType == null)
        {
            DuskPlugin.Logger.LogWarning($"Unsupported config type on ConfigReader: {_matchedEntry.SettingType.FullName}");
            onUnsupportedType?.Invoke();
            return;
        }

        if (actualType.Value != expectedType)
        {
            DuskPlugin.Logger.LogWarning($"ConfigReader type mismatch for '{pluginGuid}:{section}:{key}'. Expected '{expectedType}', but config entry was '{actualType.Value}'.");
            onTypeMismatch?.Invoke();
            return;
        }

        InvokeExpectedTypeEvent(_matchedEntry.BoxedValue);
    }

    private void InvokeExpectedTypeEvent(object value)
    {
        switch (expectedType)
        {
            case DuskDynamicConfigType.String:
                onString?.Invoke((string)value);
                return;

            case DuskDynamicConfigType.Int:
                onInt?.Invoke((int)value);
                return;

            case DuskDynamicConfigType.Float:
                onFloat?.Invoke(Convert.ToSingle(value, CultureInfo.InvariantCulture));
                return;

            case DuskDynamicConfigType.Bool:
                onBool?.Invoke((bool)value);
                return;

            case DuskDynamicConfigType.BoundedRange:
                onBoundedRange?.Invoke((BoundedRange)value);
                return;

            case DuskDynamicConfigType.Vector3:
                onVector3?.Invoke((Vector3)value);
                return;

            case DuskDynamicConfigType.Color:
                onColor?.Invoke((Color)value);
                return;

            case DuskDynamicConfigType.AnimationCurve:
                onAnimationCurve?.Invoke((AnimationCurve)value);
                return;

            default:
                DuskPlugin.Logger.LogWarning($"Unhandled expected config type: {expectedType}");
                onUnsupportedType?.Invoke();
                return;
        }
    }

    private static ConfigEntryBase? TryFindConfigEntry(string pluginGuid, string section, string key)
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
        foreach (ConfigEntryBase configEntryBase in config.GetConfigEntries())
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