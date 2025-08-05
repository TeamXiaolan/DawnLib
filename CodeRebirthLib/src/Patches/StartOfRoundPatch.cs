using System.Collections.Generic;
using System.Linq;
using CodeRebirthLib.ContentManagement.Achievements;
using CodeRebirthLib.ContentManagement.Enemies;
using CodeRebirthLib.ContentManagement.Items;
using CodeRebirthLib.Extensions;
using CodeRebirthLib.Util;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CodeRebirthLib.Patches;
static class StartOfRoundPatch
{
    internal static List<CRItemDefinition> registeredCRItems => CRMod.AllItems().ToList();

    internal static void Init()
    {
        On.StartOfRound.Awake += StartOfRound_Awake;
        On.StartOfRound.Start += StartOfRound_Start;
        On.StartOfRound.SetPlanetsWeather += StartOfRound_SetPlanetsWeather;
        On.StartOfRound.AutoSaveShipData += StartOfRound_AutoSaveShipData;
    }

    private static void StartOfRound_Awake(On.StartOfRound.orig_Awake orig, StartOfRound self)
    {
        orig(self);
        VanillaLevels.Init();
    }

    private static void StartOfRound_AutoSaveShipData(On.StartOfRound.orig_AutoSaveShipData orig, StartOfRound self)
    {
        orig(self);
        CRAchievementHandler.SaveAll();
        CodeRebirthLibNetworker.Instance?.SaveCodeRebirthLibData();
    }

    private static void StartOfRound_SetPlanetsWeather(On.StartOfRound.orig_SetPlanetsWeather orig, StartOfRound self, int connectedplayersonserver)
    {
        orig(self, connectedplayersonserver);
        CREnemyDefinition.UpdateAllWeights();
    }

    private static void StartOfRound_Start(On.StartOfRound.orig_Start orig, StartOfRound self)
    {
        CREnemyDefinition.CreateMoonAttributeStacks();
        orig(self);
        MoreLayerMasks.Init();

        self.NetworkObject.OnSpawn(() =>
        {
            if (self.IsServer || self.IsHost)
            {
                if (!CodeRebirthLibNetworker.Instance)
                {
                    GameObject networkerInstance = Object.Instantiate(CodeRebirthLibPlugin.Main.NetworkerPrefab);
                    SceneManager.MoveGameObjectToScene(networkerInstance, self.gameObject.scene);
                    networkerInstance.GetComponent<NetworkObject>().Spawn();
                }
            }

            if (AchievementUIGetCanvas.Instance == null) Object.Instantiate(CodeRebirthLibPlugin.Main.AchievementGetUICanvasPrefab);
        });
    }
}