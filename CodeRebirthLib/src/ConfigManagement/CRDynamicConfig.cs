using System;

namespace CodeRebirthLib.ConfigManagement;
[Serializable]
public class CRDynamicConfig
{
    public string settingName;
    public CRDynamicConfigType DynamicConfigType;

    public string defaultString;
    public int defaultInt;
    public float defaultFloat;
    public bool defaultBool;

    public string Description;
}