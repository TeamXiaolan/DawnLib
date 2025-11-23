using System;
using System.Collections.Generic;
using System.IO;
using Dawn.Utils;
using Unity.Netcode;
using UnityEngine;

namespace Dawn.Internal;

public class DawnNetworker : NetworkSingleton<DawnNetworker>
{
    internal static EntranceTeleport[] _entrancePoints = [];
    public static IReadOnlyList<EntranceTeleport> EntrancePoints => _entrancePoints;

    internal System.Random DawnLibRandom = new();
    internal event Action OnSave = delegate { };
    internal PersistentDataContainer SaveContainer { get; private set; }
    internal PersistentDataContainer ContractContainer { get; private set; }

    private void Awake()
    {
        if (StartOfRound.Instance == null)
            return;

        DawnLibRandom = new System.Random(StartOfRound.Instance.randomMapSeed + 6969);
        StartOfRound.Instance.StartNewRoundEvent.AddListener(OnNewRoundStart);

        string saveId = GameNetworkManager.Instance.currentSaveFileName;
        SaveContainer = CreateSaveContainer(saveId);
        ContractContainer = SaveDataPatch.contractContainer ?? CreateContractContainer(saveId);
    }

    internal static PersistentDataContainer CreateSaveContainer(string id)
    {
        PersistentDataContainer dataContainer = new PersistentDataContainer(Path.Combine(PersistentDataHandler.RootPath, $"Save{id}"));
        return dataContainer;
    }

    internal static PersistentDataContainer CreateContractContainer(string id)
    {
        return new PersistentDataContainer(Path.Combine(PersistentDataHandler.RootPath, $"Contract{id}"));
    }

    internal void SaveData()
    {
        OnSave();
        if (!NetworkManager.Singleton.IsHost)
            return;

        if (!DawnConfig.DisableDawnItemSaving)
        {
            ItemSaveDataHandler.SaveAllItems(DawnLib.GetCurrentContract()!);
        }
        if (!DawnConfig.DisableDawnUnlockableSaving)
        {
            UnlockableSaveDataHandler.SaveAllUnlockables(DawnLib.GetCurrentContract()!);
        }
    }

    private void OnNewRoundStart()
    {
        _entrancePoints = FindObjectsByType<EntranceTeleport>(FindObjectsSortMode.InstanceID);
        foreach (EntranceTeleport? entrance in EntrancePoints)
        {
            if (!entrance.FindExitPoint())
            {
                DawnPlugin.Logger.LogError("Something went wrong in the generation of the fire exits! (ignorable if EntranceTeleportOptimisation or LethalPerformance is installed)");
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void BroadcastDisplayTipServerRpc(HUDDisplayTip displayTip)
    {
        BroadcastDisplayTipClientRpc(displayTip);
    }

    [ClientRpc]
    public void BroadcastDisplayTipClientRpc(HUDDisplayTip displayTip)
    {
        HUDManager.Instance.DisplayTip(displayTip);
    }
}