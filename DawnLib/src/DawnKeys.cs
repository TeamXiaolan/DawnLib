namespace Dawn;

public static partial class DawnKeys
{
    public const string Namespace = "dawn_lib";

    // Data Keys
    public static readonly NamespacedKey LastVersion = NamespacedKey.From(Namespace, "last_version");
    public static readonly NamespacedKey DawnSave = NamespacedKey.From(Namespace, "dawn_save");
    public static readonly NamespacedKey StingerPlayed = NamespacedKey.From(Namespace, "played_stinger_once_before");
    public static readonly NamespacedKey ShipItemsSaveData = NamespacedKey.From(Namespace, "ship_items_save_data");
    public static readonly NamespacedKey ShipUnlockablesSaveData = NamespacedKey.From(Namespace, "ship_unlockables_save_data");
}