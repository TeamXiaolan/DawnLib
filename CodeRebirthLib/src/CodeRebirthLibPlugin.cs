using System;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using CodeRebirthLib.CRMod;
using PathfindingLib;
using UnityEngine;

namespace CodeRebirthLib;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency(WeatherRegistry.PluginInfo.PLUGIN_GUID, BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency(LethalConfig.PluginInfo.Guid, BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency(LethalQuantities.PluginInfo.PLUGIN_GUID, BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency(PathfindingLibPlugin.PluginGUID, BepInDependency.DependencyFlags.HardDependency)]
public class CodeRebirthLibPlugin : BaseUnityPlugin
{
    internal new static ManualLogSource Logger { get; private set; } = null!;
    internal static MainAssets Main { get; private set; } = null!;

    private void Awake()
    {
        Logger = base.Logger;
        NetcodePatcher();

        if (LethalConfigCompat.Enabled)
        {
            LethalConfigCompat.Init();
        }

        if (WeatherRegistryCompat.Enabled)
        {
            WeatherRegistryCompat.Init();
        }

        if (LethalQuantitiesCompat.Enabled)
        {
            LethalQuantitiesCompat.Init();
        }

        if (LLLCompat.Enabled)
        {
            LLLCompat.Init();
        }

        ExtendedTOML.Init();

        AdditionalTilesRegistrationHandler.Init();
        ItemRegistrationHandler.Init();
        EnemyRegistrationHandler.Init();
        UnlockableRegistrationHandler.Init();
        MapObjectRegistrationHandler.Init();

        AchievementRegistrationPatch.Init();
        CRLibNetworkerPatch.Init();
        EnemyDataPatch.Init();
        ExtraItemEventsPatch.Init();
        MiscFixesPatch.Init();
        SaveDataPatch.Init();

        DebugPrintRegistryResult("Enemies", LethalContent.Enemies, enemyInfo => enemyInfo.Enemy.enemyName);
        DebugPrintRegistryResult("Moons", LethalContent.Moons, moonInfo => moonInfo.Level.PlanetName);

        Main = new MainAssets(AssetBundleUtils.LoadBundle(Assembly.GetExecutingAssembly(), "coderebirthlibmain"));

        AutoCRModHandler.AutoRegisterMods();
    }

    private void NetcodePatcher()
    {
        var types = new Type[] { typeof(UnlockShipUnlockable), typeof(SmartAgentNavigator), typeof(CodeRebirthLibNetworker), typeof(ClientNetworkTransform), typeof(OwnerNetworkAnimator), typeof(ChanceScript), typeof(ApplyRendererVariants), typeof(NetworkAudioSource) };
        foreach (var type in types)
        {
            try
            {
                var methods = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                foreach (var method in methods)
                {
                    var attributes = method.GetCustomAttributes(typeof(RuntimeInitializeOnLoadMethodAttribute), false);
                    if (attributes.Length > 0)
                    {
                        method.Invoke(null, null);
                    }
                }
            }
            catch (Exception e)
            {
                Logger.LogWarning($"supressed an error from netcode patcher, probably fine but should still log that something happened: {e}.");
            }
        }
    }

    static void DebugPrintRegistryResult<T>(string name, Registry<T> registry, Func<T, string> nameGetter) where T : INamespaced<T>
    {
        registry.OnFreeze += () =>
        {
            Logger.LogDebug($"Registry '{name}' ({typeof(T).Name}) contains '{registry.Count}' entries.");
            foreach ((NamespacedKey<T> key, T value) in registry)
            {
                Logger.LogDebug($"{key} -> {nameGetter(value)}");
            }
        };
    }

    internal class MainAssets(AssetBundle bundle) : AssetBundleLoader<MainAssets>(bundle)
    {
        [LoadFromBundle("CodeRebirthLibNetworker.prefab")]
        public GameObject NetworkerPrefab { get; private set; } = null!;

        [LoadFromBundle("AchievementUICanvas.prefab")]
        public GameObject AchievementUICanvasPrefab { get; private set; } = null!;

        [LoadFromBundle("AchievementGetUICanvas.prefab")]
        public GameObject AchievementGetUICanvasPrefab { get; private set; } = null!;
    }
}