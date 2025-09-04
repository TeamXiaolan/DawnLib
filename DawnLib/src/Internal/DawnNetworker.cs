using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Dawn.Utils;
using GameNetcodeStuff;
using Unity.Netcode;
using UnityEngine;

namespace Dawn.Internal;

public class DawnNetworker : NetworkSingleton<DawnNetworker>
{
    internal static EntranceTeleport[] _entrancePoints = [];
    internal System.Random DawnLibRandom = new();
    internal ES3Settings SaveSettings;
    public static IReadOnlyList<EntranceTeleport> EntrancePoints => _entrancePoints;

    private void Awake()
    {
        if (StartOfRound.Instance == null)
            return;

        DawnLibRandom = new System.Random(StartOfRound.Instance.randomMapSeed + 6969);
        StartOfRound.Instance.StartNewRoundEvent.AddListener(OnNewRoundStart);
        SaveSettings = new ES3Settings($"DawnLib{GameNetworkManager.Instance.currentSaveFileName}", ES3.EncryptionType.None);
    }
    
    internal static void ResetData(ES3Settings saveSettings)
    {
        ES3.DeleteFile(saveSettings);
    }

    internal void SaveData() { }

    private void OnNewRoundStart()
    {
        _entrancePoints = FindObjectsByType<EntranceTeleport>(FindObjectsSortMode.InstanceID);
        foreach (EntranceTeleport? entrance in _entrancePoints)
        {
            if (!entrance.FindExitPoint())
            {
                DawnPlugin.Logger.LogError("Something went wrong in the generation of the fire exits! (ignorable if EntranceTeleportOptimisation is installed)");
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void BroadcastDisplayTipServerRPC(HUDDisplayTip displayTip)
    {
        BroadcastDisplayTipClientRPC(displayTip);
    }

    [ClientRpc]
    private void BroadcastDisplayTipClientRPC(HUDDisplayTip displayTip)
    {
        HUDManager.Instance.DisplayTip(displayTip);
    }
}