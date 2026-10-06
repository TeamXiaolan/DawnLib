using System;
using System.Collections.Generic;
using System.Linq;
using Dawn;
using Dawn.Internal;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Video;
using Object = UnityEngine.Object;

namespace Dusk;

internal class DuskModBundleLoader
{
    private readonly bool _hasNonPreloadAudioClips;
    private List<string> _audioClipNames = new();
    private readonly bool _hasVideoClips;
    private List<string> _videoClipNames = new();

    internal DuskModBundleLoader(DuskRegistrationContext registrationContext, AssetBundle bundle)
    {
        Debuggers.AssetLoading?.Log($"{bundle.name} contains these objects: {string.Join(",", bundle.GetAllAssetNames())}");
        foreach (Object asset in bundle.LoadAllAssets())
        {
            switch (asset)
            {
                case GameObject gameObject:
                    DawnLib.FixMixerGroups(gameObject);
                    Debuggers.AssetLoading?.Log($"Fixed Mixer Groups: {gameObject.name}");
                    if (gameObject.GetComponent<NetworkObject>() == null)
                        continue;

                    DawnLib.RegisterNetworkPrefab(gameObject);
                    Debuggers.AssetLoading?.Log($"Registered Network Prefab: {gameObject.name}");
                    break;
                case VideoClip videoClip:
                    _videoClipNames.Add(videoClip.name);
                    _hasVideoClips = true;
                    break;
                case AudioClip audioClip:
                    if (audioClip.preloadAudioData)
                        continue;

                    _audioClipNames.Add(audioClip.name);
                    _hasNonPreloadAudioClips = true;
                    break;
            }
        }

        List<DuskContentDefinition> Content = bundle.LoadAllAssets<DuskContentDefinition>().ToList();

        // Sort content
        List<Type> definitionOrder = [
            typeof(DuskMoonDefinition),
            typeof(DuskDungeonDefinition),
            typeof(DuskWeatherDefinition),
            typeof(DuskVehicleDefinition),
            typeof(DuskMapObjectDefinition),
            typeof(DuskEnemyDefinition),
            typeof(DuskUnlockableDefinition),
            typeof(DuskItemDefinition),
            typeof(DuskTerminalCommandDefinition),
            typeof(DuskEntityReplacementDefinition),
            typeof(DuskStoryLogDefinition),
            typeof(DuskAchievementDefinition),
            typeof(DuskSurfaceDefinition),
            typeof(DuskAdditionalTilesDefinition),
        ];

        Content = Content.OrderBy(it =>
        {
            Type definitionType = it.GetType();
            int index = definitionOrder.IndexOf(definitionType);
            return index >= 0 ? index : int.MaxValue;
        }).ToList();

        Dictionary<DuskContentDefinition, List<DuskConfigDefinition>> configDefinitions = new();
        foreach (DuskContentDefinition definition in Content)
        {
            if (definition is not DuskConfigDefinition configDefinition)
            {
                if (definition._configEntries == null || definition._configEntries.Count == 0)
                {
                    continue;
                }

                foreach (DuskDynamicConfig dynamicConfig in definition._configEntries)
                {
                    DuskConfigDefinition newConfigDefinition = DuskDynamicConfig.CreateConfigDefinitionFromDynamicConfig(definition, dynamicConfig);
                    if (!configDefinitions.ContainsKey(definition))
                    {
                        configDefinitions[definition] = new List<DuskConfigDefinition>();
                    }
                    configDefinitions[definition].Add(newConfigDefinition);
                }
                continue;
            }

            if (configDefinition.ContentReference == null)
            {
                continue;
            }

            if (!configDefinitions.ContainsKey(configDefinition.ContentReference))
            {
                configDefinitions[configDefinition.ContentReference] = new List<DuskConfigDefinition>();
            }
            configDefinitions[configDefinition.ContentReference].Add(configDefinition);
        }

        foreach (DuskContentDefinition definition in Content)
        {
            if (definition is DuskConfigDefinition configDefinition)
            {
                continue;
            }

            definition.Register(registrationContext);
            if (configDefinitions.TryGetValue(definition, out List<DuskConfigDefinition>? configs))
            {
                foreach (DuskConfigDefinition config in configs)
                {
                    config.Register(registrationContext);
                }
            }
            definition.RegisterPost(registrationContext);
        }

        if (_hasNonPreloadAudioClips)
        {
            DuskPlugin.Logger.LogWarning($"Bundle: '{bundle.name}' is being unloaded but contains atleast one AudioClip that has 'preloadAudioData' to false! This will cause errors when trying to play said AudioClips, unloading stopped.");
            foreach (string audioClipName in _audioClipNames)
            {
                Debuggers.AssetLoading?.Log($"AudioClip Name: {audioClipName}");
            }
        }

        if (_hasVideoClips)
        {
            foreach (string videoClipName in _videoClipNames)
            {
                Debuggers.AssetLoading?.Log($"VideoClip Name: {videoClipName}");
            }
            DuskPlugin.Logger.LogInfo($"Bundle: '{bundle.name}' is no longer being unloaded due to containing atleast one VideoClip.");
            DuskPlugin.Logger.LogInfo($"I recommend, if possible, placing the VideoClip into an entirely separate AssetBundle and loading it manually onto where you need it to be.");
            return;
        }

        bundle.Unload(false);
    }
}