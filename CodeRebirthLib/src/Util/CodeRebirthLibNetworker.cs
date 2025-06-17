using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace CodeRebirthLib.Util;

public class CodeRebirthLibNetworker : NetworkBehaviour
{
    internal static EntranceTeleport[] entrancePoints = [];
    internal ES3Settings SaveSettings;
    internal static Dictionary<EnemyAI, ExtraEnemyData> ExtraEnemyDataDict = new();
    internal System.Random CRLibRandom = new();
    internal static CodeRebirthLibNetworker Instance { get; private set; } = null!;

    private void Awake()
    {
        Instance = this;
        CRLibRandom = new System.Random(StartOfRound.Instance.randomMapSeed + 6969);
        StartOfRound.Instance.StartNewRoundEvent.AddListener(OnNewRoundStart);
        SaveSettings = new($"CRLib{GameNetworkManager.Instance.currentSaveFileName}", ES3.EncryptionType.None);
        StartCoroutine(ProgressiveUnlockables.LoadUnlockedIDs(false)); // TODO, rewrite this
    }

    public void OnNewRoundStart()
    {
        entrancePoints = FindObjectsByType<EntranceTeleport>(FindObjectsSortMode.InstanceID);
        foreach (var entrance in entrancePoints)
        {
            if (!entrance.FindExitPoint())
            {
                CodeRebirthLibPlugin.Logger.LogError("Something went wrong in the generation of the fire exits! (ignorable if EntranceTeleportOptimisation is installed)");
            }
        }
    }
}