using System;

namespace CodeRebirthLib.ConfigManagement;
[Serializable]
public class CRDynamicConfig
{
    public string settingName;
    public string settingDesc;
    public CRDynamicConfigType DynamicConfigType;

    public string defaultString;
    public int defaultInt;
    public float defaultFloat;
    public bool defaultBool;

    public string Description;

    public CRDynamicConfig? GetCRDynamicConfigWithSetting(string _settingName, string _settingDesc)
    {
        if (_settingName.Contains(settingName, StringComparison.OrdinalIgnoreCase)
            && _settingDesc.Contains(settingDesc, StringComparison.OrdinalIgnoreCase))
            return this;
        return null;
    }
}