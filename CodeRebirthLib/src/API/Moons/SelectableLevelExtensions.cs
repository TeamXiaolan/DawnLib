using System;
using System.Reflection;
using CodeRebirthLib.Internal;

namespace CodeRebirthLib;

public static class SelectableLevelExtensions
{
    // todo: reference stripped patched assembly??
    private static FieldInfo _infoField = typeof(SelectableLevel).GetField("__crinfo", BindingFlags.Instance | BindingFlags.Public);

    public static NamespacedKey<CRMoonInfo>? ToNamespacedKey(this SelectableLevel level)
    {
        if (!level.HasCRInfo())
        {
            Debuggers.Moons?.Log($"Registering potentially modded level: {level.PlanetName}");
            NamespacedKey<CRMoonInfo> key = NamespacedKey<CRMoonInfo>.From("lethal_level_loader", NamespacedKey.NormalizeStringForNamespacedKey(level.PlanetName, false));
            CRMoonInfo moonInfo = new(key, [CRLibTags.IsExternal], level);
            level.SetCRInfo(moonInfo);
            LethalContent.Moons.Register(moonInfo);
            // throw new MissingFieldException(); // TODO what exception should this be throwing
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
