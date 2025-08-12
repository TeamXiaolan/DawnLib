using System.Globalization;
using System.Linq;
using System.Reflection;

namespace CodeRebirthLib;

public static class SelectableLevelExtensions
{
    // todo: reference stripped patched assembly??
    private static FieldInfo _infoField = typeof(SelectableLevel).GetField("__crinfo", BindingFlags.Instance | BindingFlags.NonPublic);
    
    public static NamespacedKey<CRMoonInfo> ToNamespacedKey(this SelectableLevel level)
    {
        return level.GetCRInfo().TypedKey;
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
