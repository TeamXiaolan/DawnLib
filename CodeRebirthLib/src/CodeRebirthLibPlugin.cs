using System;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using CodeRebirthLib.Dungeons;
using UnityEngine;

namespace CodeRebirthLib;
[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class CodeRebirthLibPlugin : BaseUnityPlugin
{
    internal new static ManualLogSource Logger { get; private set; } = null!;

    private void Awake()
    {
        Logger = base.Logger;
        ItemRegistrationHandler.Init();
        EnemyRegistrationHandler.Init();
        
        CRLib.DefineTileSet(NamespacedKey<CRTileSetInfo>.From("code_rebirth", "extra_facility_tiles"), builder => builder
            .AddToDungeon(DungeonKeys.Facility)
            .SetIsRegular(false)
            .SetIsBranchCap(true)
        );
        
        TomlTypeConverter.AddConverter(typeof(NamespacedKey<CRMoonInfo>),
            new TypeConverter()
            {
                ConvertToObject = (str, type) => NamespacedKey<CRMoonInfo>.Parse(str),
                ConvertToString = (obj, type) => obj.ToString()
            }
        );
        Config.Bind("bwaa", "bwa", NamespacedKey<CRMoonInfo>.From("bwaa", "bwaa"), "bwaaa");

        DebugPrintRegistryResult("Enemies", LethalContent.Enemies, enemyInfo => enemyInfo.Enemy.enemyName);
        DebugPrintRegistryResult("Moons", LethalContent.Moons, moonInfo => moonInfo.Level.PlanetName);

        AutoCRModHandler.AutoRegisterMods();
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
}