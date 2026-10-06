using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using Dawn.Internal;
using UnityEngine;

namespace Dusk;

public class DuskMod
{
    public const string PLUGIN_GUID = MyPluginInfo.PLUGIN_GUID;

    private static readonly List<DuskMod> _allMods = new();
    internal List<ConfigEntryBase> configEntries = new();

    public static event Action<DuskMod> BeforeRegisterDuskMod
    {
        add
        {
            _beforeRegisterDuskMod += duskMod =>
            {
                try
                {
                    value(duskMod);
                }
                catch (Exception exception)
                {
                    DuskPlugin.Logger.LogError($"(BeforeRegisterDuskMod) An exception occured in firing an event for a duskMod:\n{exception}");
                }
            };
        }
        remove => DuskPlugin.Logger.LogError("DuskMod.BeforeRegisterDuskMod -= is not supported.");
    }

    public static event Action<DuskMod> OnRegisterDuskMod
    {
        add
        {
            _onRegisterDuskMod += duskMod =>
            {
                try
                {
                    value(duskMod);
                }
                catch (Exception exception)
                {
                    DuskPlugin.Logger.LogError($"(OnRegisterDuskMod) An exception occured in firing an event for a duskMod:\n{exception}");
                }
            };
        }
        remove => DuskPlugin.Logger.LogError("DuskMod.OnRegisterDuskMod -= is not supported.");
    }
    private static event Action<DuskMod> _onRegisterDuskMod = delegate { }, _beforeRegisterDuskMod = delegate { };

    private readonly string _basePath;

    internal static DuskMod RegisterNoCodeMod(DuskModInformation modInfo, AssetBundle mainBundle, string basePath)
    {
        BepInPlugin plugin = (BepInPlugin)modInfo.CreatePluginMetadata();
        Debuggers.Dusk?.Log("Registering no-code mod!");
        ConfigManager configManager;
        if (string.IsNullOrEmpty(modInfo.ConfigFileName))
        {
            configManager = new ConfigManager(ConfigManager.GenerateConfigFile(plugin));
        }
        else
        {
            configManager = new ConfigManager(ConfigManager.GenerateConfigFile(modInfo.ConfigFileName));
        }

        DuskMod noCodeMod = new(plugin, mainBundle, basePath, configManager)
        {
            ModInformation = modInfo,
            Logger = BepInEx.Logging.Logger.CreateLogSource(plugin.GUID)
        };

        _beforeRegisterDuskMod(noCodeMod);
        TryRegisterContent(noCodeMod);
        _onRegisterDuskMod(noCodeMod);

        if (DuskLethalConfigCompat.Enabled)
        {
            DuskLethalConfigCompat.CreateLethalConfigMod(noCodeMod);
        }
        return noCodeMod;
    }

    private static void TryRegisterContent(DuskMod duskMod)
    {
        duskMod.Logger.LogDebug($"Trying to register bundle: {duskMod.Content.name} with {duskMod.Content.assetBundles.Count} assets.");
        foreach (AssetBundleData bundleData in duskMod.Content.assetBundles)
        {
            if (!IsContentEnabled(duskMod, bundleData))
                continue;

            if (!duskMod.TryGetRelativeFile(out string path, "Assets", bundleData.assetBundleName))
            {
                duskMod.Logger.LogError($"The bundle: {bundleData.configName} is not defined at plugins/{Path.GetRelativePath(Paths.PluginPath, path)}.");

                if (duskMod.TryGetRelativeFile(out string incorrectPath, bundleData.assetBundleName)) // check if it is instead next to the .duskmod file
                {
                    duskMod.Logger.LogError($"The bundle is instead defined at plugins/{Path.GetRelativePath(Paths.PluginPath, incorrectPath)}. It should be in an Assets/ subfolder.");
                }

                duskMod.Logger.LogError("Please make sure that you uploaded your mod zip correctly.");
                duskMod.Logger.LogError("Local mods do not have their folder structure edited, but uploaded mods CAN if you don't follow a specific naming scheme");
                duskMod.Logger.LogError(
                    "Here is a working example:\n" +
                    "Folder Structure:\n" +
                    "YOURzip\n" +
                    "└─ plugins\n" +
                    "    └─ Assets\n" +
                    "        └─ NormalAssetBundlesGoHere\n" +
                    "    └─ DuskModAssetBundleGoesHere\n" +
                    "└─ CHANGELOG.md\n" +
                    "└─ icon.md\n" +
                    "└─ LICENSE.md\n" +
                    "└─ manifest.json\n" +
                    "└─ README.md\n"
                    );
                continue;
            }

            DuskRegistrationContext registrationContext = new(duskMod, bundleData);
            new DuskModBundleLoader(registrationContext, AssetBundle.LoadFromFile(path));
        }
    }

    [MethodImpl(MethodImplOptions.NoOptimization | MethodImplOptions.NoInlining)]
    private static bool IsContentEnabled(DuskMod duskMod, AssetBundleData assetBundleData)
    {
        using ConfigContext section = duskMod.ConfigManager.CreateConfigSectionForBundleData(assetBundleData);
        string configName = assetBundleData.configName;
        ConfigEntry<bool> isEnabled = section.Bind("Enabled", $"Whether {configName} is enabled.", assetBundleData.enabledByDefault);
        duskMod.configEntries.Add(isEnabled);
        return isEnabled.Value;
    }

    internal DuskMod(BepInPlugin plugin, AssetBundle mainBundle, string basePath, ConfigManager configManager)
    {
        ConfigManager = configManager;
        _basePath = basePath;
        Plugin = plugin;

        ContentContainer[] containers = mainBundle.LoadAllAssets<ContentContainer>();
        if (containers.Length == 0)
        {
            throw new NoContentDefinitionInBundle(mainBundle);
        }
        if (containers.Length >= 2)
        {
            throw new MultipleContentDefinitionsInBundle(mainBundle);
        }

        Content = containers[0];
        _allMods.Add(this);
    }

    public static IReadOnlyList<DuskMod> AllMods => _allMods.AsReadOnly();

    public IReadOnlyList<ConfigEntryBase> ConfigEntries => configEntries.AsReadOnly();
    public ConfigManager ConfigManager { get; }
    public ContentContainer Content { get; }

    public DuskModInformation ModInformation { get; set; }
    public ManualLogSource Logger { get; set; }

    public BepInPlugin Plugin { get; }

    public string GetRelativePath(params string[] path)
    {
        return Path.Combine(_basePath, Path.Combine(path));
    }

    public bool TryGetRelativeFile(out string fullPath, params string[] path)
    {
        fullPath = GetRelativePath(path);
        return File.Exists(fullPath);
    }

    public bool TryGetBundleDataFromName(string bundleName, [NotNullWhen(true)] out AssetBundleData? data)
    {
        data = Content.assetBundles.FirstOrDefault(it => it.assetBundleName == bundleName);
        return data != null;
    }
}