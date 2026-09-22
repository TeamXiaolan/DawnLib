using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using BepInEx.Bootstrap;
using DunGen.Graph;
using DunGenPlus;
using UnityEngine;

namespace Dawn.Internal;

static class DungeonGenerationPlusCompat
{
    internal const string COMPATIBLE_VERSION = "1.5.0";
    public static bool Enabled => Chainloader.PluginInfos.ContainsKey("dev.ladyalice.dungenplus") && Chainloader.PluginInfos["dev.ladyalice.dungenplus"].Metadata.Version >= Version.Parse(COMPATIBLE_VERSION);
    internal static bool RemovedHotloading = false;

    private static Dictionary<string, object> extenderObjectToInteriors = new();
    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    internal static void HandleExtenderForBundle(AssetBundle assetBundle, DungeonFlow dungeonFlow, bool register)
    {
        if (register)
        {
            DunGenExtender[] extenders = assetBundle.LoadAllAssets<DunGenExtender>();
            if (extenders.Length == 0)
            {
                return;
            }

            if (extenders.Length > 1)
            {
                DawnPlugin.Logger.LogWarning("Multiple DunGenExtender assets found in bundle " + assetBundle.name + ", there should only be one.");
            }

            extenderObjectToInteriors[dungeonFlow.name] = extenders[0];
            extenders[0].DungeonFlow = dungeonFlow;
            DunGenPlus.API.AddDunGenExtender(dungeonFlow, extenders[0]);
        }
        else
        {
            if (extenderObjectToInteriors.TryGetValue(dungeonFlow.name, out object extenderObject))
            {
                DunGenPlus.API.RemoveDunGenExtender(dungeonFlow);
                extenderObjectToInteriors.Remove(dungeonFlow.name);
            }
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    internal static bool IsDebugOn()
    {
        if (PluginConfig.EnableDevDebugTools.Value)
        {
            return true;
        }
        return false;
    }

    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    internal static void ReloadMainPanel()
    {
        try
        {
            GameObject.FindFirstObjectByType<DunGenPlus.DevTools.Panels.MainPanel>().UpdatePanel();
        }
        catch { }
    }
}