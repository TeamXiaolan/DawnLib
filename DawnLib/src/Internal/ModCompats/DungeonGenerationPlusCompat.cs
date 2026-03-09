using System.Runtime.CompilerServices;
using BepInEx.Bootstrap;
using DunGen.Graph;
using DunGenPlus;
using UnityEngine;

namespace Dawn.Internal;

static class DungeonGenerationPlusCompat
{
    public static bool Enabled => Chainloader.PluginInfos.ContainsKey("dev.ladyalice.dungenplus");


    private static object? extenderObject = null;
    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    public static void RegisterExtenderForBundle(AssetBundle assetBundle, DungeonFlow dungeonFlow, bool register)
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

            extenderObject = extenders[0];
            ((DunGenExtender)extenderObject).DungeonFlow = dungeonFlow;
            DunGenPlus.API.AddDunGenExtender(dungeonFlow, (DunGenExtender)extenderObject);
        }
        else
        {
            if (extenderObject == null)
            {
                return;
            }

            DunGenPlus.API.RemoveDunGenExtender(dungeonFlow);
            extenderObject = null;
        }
    }
}