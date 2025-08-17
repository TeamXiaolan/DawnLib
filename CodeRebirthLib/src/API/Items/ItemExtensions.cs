using System.Reflection;

namespace CodeRebirthLib;

public static class ItemExtensions
{
    // todo: reference stripped patched assembly??
    private static FieldInfo _infoField = typeof(Item).GetField("__crinfo", BindingFlags.Instance | BindingFlags.Public);

    public static NamespacedKey<CRItemInfo>? ToNamespacedKey(this Item item)
    {
        if (!item.HasCRInfo())
        {
            CodeRebirthLibPlugin.Logger.LogError($"Item '{item.itemName}' does not have a CRItemInfo, you are either accessing this too early or it erroneously never got created!");
            return null;
        }
        return item.GetCRInfo().TypedKey;
    }

    internal static bool HasCRInfo(this Item item)
    {
        return _infoField.GetValue(item) != null;
    }

    internal static CRItemInfo GetCRInfo(this Item item)
    {
        return (CRItemInfo)_infoField.GetValue(item);
    }

    internal static void SetCRInfo(this Item item, CRItemInfo info)
    {
        _infoField.SetValue(item, info);
    }
}
