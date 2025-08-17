using System.Reflection;

namespace CodeRebirthLib;

public static class EnemyTypeExtensions
{
    // todo: reference stripped patched assembly??
    private static FieldInfo _infoField = typeof(EnemyType).GetField("__crinfo", BindingFlags.Instance | BindingFlags.Public);

    public static NamespacedKey<CREnemyInfo>? ToNamespacedKey(this EnemyType enemyType)
    {
        if (!enemyType.HasCRInfo())
        {
            CodeRebirthLibPlugin.Logger.LogError($"EnemyType '{enemyType.enemyName}' does not have a CREnemyInfo, you are either accessing this too early or it erroneously never got created!");
            return null;
        }
        return enemyType.GetCRInfo().TypedKey;
    }

    internal static bool HasCRInfo(this EnemyType enemyType)
    {
        return _infoField.GetValue(enemyType) != null;
    }

    internal static CREnemyInfo GetCRInfo(this EnemyType enemyType)
    {
        return (CREnemyInfo)_infoField.GetValue(enemyType);
    }

    internal static void SetCRInfo(this EnemyType enemyType, CREnemyInfo info)
    {
        _infoField.SetValue(enemyType, info);
    }
}
