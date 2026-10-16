using System;
using Dawn;
using Dawn.Utils;
using Dusk;
using UnityEngine;

namespace Sundown;

[Serializable]
public class SundownDynamicConfig
{
    public string settingName;
    public SundownDynamicConfigType DynamicConfigType;

    public string defaultString;
    public int defaultInt;
    public float defaultFloat;
    public bool defaultBool;
    public BoundedRange defaultBoundedRange;
    public AnimationCurve defaultAnimationCurve;
    public Vector3 defaultVector3;
    public Color defaultColor;

    public string Description;

    internal static SundownConfigDefinition CreateConfigDefinitionFromDynamicConfig(DuskContentDefinition contentReference, SundownDynamicConfig dynamicConfig)
    {
        SundownConfigDefinition configDefinition = ScriptableObject.CreateInstance<SundownConfigDefinition>();
        configDefinition.name = dynamicConfig.settingName.Replace(" ", "") + "ConfigDefinition";
        configDefinition.DynamicConfig = dynamicConfig;
        configDefinition.ContentReference = contentReference;
        configDefinition._typedKey = NamespacedKey.From(contentReference.Key.Namespace, dynamicConfig.settingName).AsTyped<SundownConfigDefinition>();
        return configDefinition;
    }
}