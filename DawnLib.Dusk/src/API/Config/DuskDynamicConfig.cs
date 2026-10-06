using System;
using Dawn;
using Dawn.Utils;
using UnityEngine;

namespace Dusk;

[Serializable]
public class DuskDynamicConfig
{
    public string settingName;
    public DuskDynamicConfigType DynamicConfigType;

    public string defaultString;
    public int defaultInt;
    public float defaultFloat;
    public bool defaultBool;
    public BoundedRange defaultBoundedRange;
    public AnimationCurve defaultAnimationCurve;
    public Vector3 defaultVector3;
    public Color defaultColor;

    public string Description;

    internal static DuskConfigDefinition CreateConfigDefinitionFromDynamicConfig(DuskContentDefinition contentReference, DuskDynamicConfig dynamicConfig)
    {
        DuskConfigDefinition configDefinition = ScriptableObject.CreateInstance<DuskConfigDefinition>();
        configDefinition.name = dynamicConfig.settingName.Replace(" ", "") + "ConfigDefinition";
        configDefinition.DynamicConfig = dynamicConfig;
        configDefinition.ContentReference = contentReference;
        configDefinition._typedKey = NamespacedKey.From(contentReference.Key.Namespace, dynamicConfig.settingName).AsTyped<DuskConfigDefinition>();
        return configDefinition;
    }
}