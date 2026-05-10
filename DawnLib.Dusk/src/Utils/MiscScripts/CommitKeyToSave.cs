using System.Collections.Generic;
using Dawn;
using Unity.Netcode;
using UnityEngine;

namespace Dusk.Utils;

public enum SaveTypes
{
    Contract,
    Save
}

public class CommitKeyToSave : MonoBehaviour
{
    internal static readonly NamespacedKey DawnLibLoreKey = NamespacedKey.From("dawn_lib", "lore_collection");

    [field: SerializeField]
    public SaveTypes SaveType { get; private set; } = SaveTypes.Contract;

    [field: SerializeField]
    public string LoreEntryName { get; private set; } = string.Empty;

    [field: SerializeField, InspectorName("Namespace"), DefaultKeySource("GetLoreEntryName", false)]
    public NamespacedKey NamespacedKey { get; private set; }

    [field: SerializeField, TextArea(2, 10)]
    public string FoundEntryText { get; private set; } = "Found journal entry: ' '";

    [field: SerializeField]
    public bool SaveImmediately { get; private set; } = false;

    private static HashSet<NamespacedKey> _savedContractKeys = new();
    private static HashSet<NamespacedKey> _savedSaveKeys = new();

    private static bool _shouldSaveContract = false;
    private static bool _shouldSaveSave = false;

    internal static void Init()
    {
        On.GameNetworkManager.SaveItemsInShip += SaveKeys;
        On.StartOfRound.AutoSaveShipData += SaveKeys;
    }

    private static void SaveKeys(On.StartOfRound.orig_AutoSaveShipData orig, StartOfRound self)
    {
        orig(self);
        SaveKeys();
    }

    private static void SaveKeys(On.GameNetworkManager.orig_SaveItemsInShip orig, GameNetworkManager self)
    {
        orig(self);
        SaveKeys();
    }

    public void CommitKey()
    {
        if (!NetworkManager.Singleton.IsServer)
        {
            return;
        }

        CommitNamespaceIntoFile(NamespacedKey);
    }

    public void Destroy()
    {
        Destroy(gameObject);
    }

    private void CommitNamespaceIntoFile(NamespacedKey key)
    {
        switch (SaveType)
        {
            case SaveTypes.Contract:
                _savedContractKeys = DawnLib.GetCurrentContract()!.GetOrCreateDefault<HashSet<NamespacedKey>>(DawnLibLoreKey);
                if (!_savedContractKeys.Add(key))
                {
                    return;
                }
                _shouldSaveContract = true;
                break;
            case SaveTypes.Save:
                _savedSaveKeys = DawnLib.GetCurrentSave()!.GetOrCreateDefault<HashSet<NamespacedKey>>(DawnLibLoreKey);
                if (!_savedSaveKeys.Add(key))
                {
                    return;
                }
                _shouldSaveSave = true;
                break;
        }

        HUDManager.Instance.DisplayGlobalNotification(FoundEntryText);
        if (!SaveImmediately && !StartOfRound.Instance.inShipPhase)
        {
            return;
        }

        SaveKeys();
    }

    private static void SaveKeys()
    {
        if (_shouldSaveContract)
        {
            DawnLib.GetCurrentContract()!.Set(DawnLibLoreKey, _savedContractKeys);
        }

        if (_shouldSaveSave)
        {
            DawnLib.GetCurrentSave()!.Set(DawnLibLoreKey, _savedSaveKeys);
        }

        _shouldSaveContract = false;
        _shouldSaveSave = false;
    }

    public string GetLoreEntryName()
    {
        string normalizedName = NamespacedKey.NormalizeStringForNamespacedKey(LoreEntryName, false);
        return normalizedName;
    }
}