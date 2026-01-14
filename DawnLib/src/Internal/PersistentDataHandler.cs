using System.Collections.Generic;
using System.IO;
using BepInEx;
using UnityEngine;

namespace Dawn.Internal;
static class PersistentDataHandler
{
    private static Dictionary<BaseUnityPlugin, PersistentDataContainer> _containers = [];

    internal static readonly string RootPath = Path.Combine(Application.persistentDataPath, "DawnLib");

    internal static void Init()
    {
        Directory.CreateDirectory(Path.Combine(RootPath, "PluginData"));
    }

    internal static PersistentDataContainer Get(BaseUnityPlugin plugin)
    {
        if (_containers.TryGetValue(plugin, out PersistentDataContainer? value))
        {
            return value;
        }
        BepInPlugin pluginInfo = MetadataHelper.GetMetadata(plugin.GetType());

        value = new PersistentDataContainer(Path.Combine(RootPath, "PluginData", $"{pluginInfo.GUID}.dawndata"));
        _containers[plugin] = value;
        return value;
    }
}