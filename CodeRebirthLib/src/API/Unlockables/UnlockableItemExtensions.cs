using System.Reflection;

namespace CodeRebirthLib;

public static class UnlockableItemExtensions
{
    // todo: reference stripped patched assembly??
    private static FieldInfo _infoField = typeof(UnlockableItem).GetField("__crinfo", BindingFlags.Instance | BindingFlags.Public);

    public static NamespacedKey<CRUnlockableItemInfo>? ToNamespacedKey(this UnlockableItem unlockableItem)
    {
        if (!unlockableItem.HasCRInfo())
        {
            CodeRebirthLibPlugin.Logger.LogError($"UnlockableItem '{unlockableItem.unlockableName}' does not have a CRUnlockableItemInfo, you are either accessing this too early or it erroneously never got created!");
            return null;
        }
        return unlockableItem.GetCRInfo().TypedKey;
    }

    internal static bool HasCRInfo(this UnlockableItem unlockableItem)
    {
        return _infoField.GetValue(unlockableItem) != null;
    }

    internal static CRUnlockableItemInfo GetCRInfo(this UnlockableItem unlockableItem)
    {
        return (CRUnlockableItemInfo)_infoField.GetValue(unlockableItem);
    }

    internal static void SetCRInfo(this UnlockableItem unlockableItem, CRUnlockableItemInfo info)
    {
        _infoField.SetValue(unlockableItem, info);
    }
}
