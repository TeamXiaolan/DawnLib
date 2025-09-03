using System;
using Dawn.Utils;
using UnityEngine;

namespace Dawn.Dusk;
[Serializable]
public class CRDynamicConfig
{
    public string settingName;
    public CRDynamicConfigType DynamicConfigType;

    public string defaultString;
    public int defaultInt;
    public float defaultFloat;
    public bool defaultBool;
    public BoundedRange defaultBoundedRange;
    public AnimationCurve defaultAnimationCurve;

    public string Description;
}