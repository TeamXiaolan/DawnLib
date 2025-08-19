using System.Globalization;
using System.Linq;
using System.Reflection;

namespace CodeRebirthLib;

public static class SelectableLevelExtensions
{
    // todo: reference stripped patched assembly??
    private static FieldInfo _infoField = typeof(SelectableLevel).GetField("__crinfo", BindingFlags.Instance | BindingFlags.Public);

    public static NamespacedKey<CRMoonInfo>? ToNamespacedKey(this SelectableLevel level)
    {
        if (!level.HasCRInfo())
        {
            CodeRebirthLibPlugin.Logger.LogError($"SelectableLevel '{level.PlanetName}' does not have a CRMoonInfo, you are either accessing this too early or it erroneously never got created!");
            return null;
        }
        return level.GetCRInfo().TypedKey;
    }

    internal static bool HasCRInfo(this SelectableLevel level)
    {
        return _infoField.GetValue(level) != null;
    }

    internal static CRMoonInfo GetCRInfo(this SelectableLevel level)
    {
        return (CRMoonInfo)_infoField.GetValue(level);
    }

    internal static void SetCRInfo(this SelectableLevel level, CRMoonInfo info)
    {
        _infoField.SetValue(level, info);
    }
}
