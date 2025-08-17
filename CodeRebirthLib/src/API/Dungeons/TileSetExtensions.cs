using System.Reflection;
using DunGen;

namespace CodeRebirthLib;

public static class TileSetExtensions
{
    // todo: reference stripped patched assembly??
    private static FieldInfo _infoField = typeof(TileSet).GetField("__crinfo", BindingFlags.Instance | BindingFlags.Public);

    public static NamespacedKey<CRTileSetInfo>? ToNamespacedKey(this TileSet tileSet)
    {
        if (!tileSet.HasCRInfo())
        {
            CodeRebirthLibPlugin.Logger.LogError($"TileSet '{tileSet.name}' does not have a CRTileSetInfo, you are either accessing this too early or it erroneously never got created!");
            return null;
        }
        return tileSet.GetCRInfo().TypedKey;
    }

    internal static bool HasCRInfo(this TileSet tileSet)
    {
        return _infoField.GetValue(tileSet) != null;
    }

    internal static CRTileSetInfo GetCRInfo(this TileSet tileSet)
    {
        return (CRTileSetInfo)_infoField.GetValue(tileSet);
    }

    internal static void SetCRInfo(this TileSet tileSet, CRTileSetInfo info)
    {
        _infoField.SetValue(tileSet, info);
    }
}
