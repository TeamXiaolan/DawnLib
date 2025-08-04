using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CodeRebirthLib.ContentManagement;
using CodeRebirthLib.ContentManagement.Enemies;
using CodeRebirthLib.ContentManagement.Items;
using CodeRebirthLib.ContentManagement.MapObjects;
using CodeRebirthLib.ContentManagement.Unlockables;
using CodeRebirthLib.ContentManagement.Weathers;
using CodeRebirthLib.Patches;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Video;
using Object = UnityEngine.Object;

namespace CodeRebirthLib.AssetManagement;
public abstract class AssetBundleLoader<T> : IAssetBundleLoader where T : AssetBundleLoader<T>
{
    private readonly bool _hasNonPreloadAudioClips;
    private List<string> _audioClipNames = new();
    private readonly bool _hasVideoClips;
    private List<string> _videoClipNames = new();

    private AssetBundle? _bundle;

    protected AssetBundleLoader(CRMod mod, string filePath) : this(mod.Assembly, filePath)
    {
    }

    internal AssetBundleLoader(Assembly assembly, string filePath) : this(CRLib.LoadBundle(assembly, filePath))
    {
    }

    protected AssetBundleLoader(AssetBundle bundle)
    {
        _bundle = bundle;

        CodeRebirthLibPlugin.ExtendedLogging($"[AssetBundle Loading] {bundle.name} contains these objects: {string.Join(",", bundle.GetAllAssetNames())}");
        
        Type type = typeof(T);
        foreach (PropertyInfo property in type.GetProperties())
        {
            LoadFromBundleAttribute loadInstruction = (LoadFromBundleAttribute)property.GetCustomAttribute(typeof(LoadFromBundleAttribute));
            if (loadInstruction == null) continue;

            property.SetValue(this, LoadAsset(bundle, loadInstruction.BundleFile));
        }

        foreach (Object asset in bundle.LoadAllAssets())
        {
            switch (asset)
            {
                case GameObject gameObject:
                    CRLib.FixMixerGroups(gameObject);
                    CodeRebirthLibPlugin.ExtendedLogging($"[AssetBundle Loading] Fixed Mixer Groups: {gameObject.name}");

                    if (gameObject.GetComponent<NetworkObject>() == null)
                        continue;

                    CRLib.RegisterNetworkPrefab(gameObject);
                    CodeRebirthLibPlugin.ExtendedLogging($"[AssetBundle Loading] Registered Network Prefab: {gameObject.name}");
                    break;
                case VideoClip videoClip:
                    _videoClipNames.Add(videoClip.name);
                    _hasVideoClips = true;
                    break;
                case AudioClip audioClip:
                    if (audioClip.preloadAudioData)
                        break;

                    _audioClipNames.Add(audioClip.name);
                    _hasNonPreloadAudioClips = true;
                    break;
            }
        }

        Content = bundle.LoadAllAssets<CRContentDefinition>();
        
        // Sort content
        List<Type> definitionOrder = [
            typeof(CREnemyDefinition),
            typeof(CRItemDefinition),
            typeof(CRMapObjectDefinition),
            typeof(CRUnlockableDefinition),
            typeof(CRWeatherDefinition)
        ];
        Content = Content.OrderBy(it =>
        {
            Type definitionType = it.GetType();
            int index = definitionOrder.IndexOf(definitionType);
            return index >= 0 ? index : int.MaxValue;
        }).ToArray();
    }

    public AssetBundleData? AssetBundleData { get; set; } = null;
    public CRContentDefinition[] Content { get; }

    internal void TryUnload()
    {
        if (AssetBundleData?.AlwaysKeepLoaded ?? true)
            return;

        if (_bundle == null)
        {
            CodeRebirthLibPlugin.Logger.LogError("Tried to unload bundle twice?");
            throw new NullReferenceException();
        }

        if (_hasVideoClips)
        {
            CodeRebirthLibPlugin.Logger.LogWarning($"Bundle: '{_bundle.name}' has at least one VideoClip but is being unloaded! Playing video clips from this bundle could cause errors! Mark `AlwaysKeepLoaded` as true to stop this from happening.");
            foreach (string videoClipName in _videoClipNames)
            {
                CodeRebirthLibPlugin.ExtendedLogging($"VideoClip Name: {videoClipName}");
            }
        }

        if (_hasNonPreloadAudioClips)
        {
            CodeRebirthLibPlugin.Logger.LogWarning($"Bundle: '{_bundle.name}' is being unloaded but contains an AudioClip that has 'preloadAudioData' to false! This will cause errors when trying to play this clip.");
            foreach (string audioClipName in _audioClipNames)
            {
                CodeRebirthLibPlugin.ExtendedLogging($"AudioClip Name: {audioClipName}");
            }
        }

        _bundle.Unload(false);
        _bundle = null;
    }

    private Object LoadAsset(AssetBundle bundle, string path)
    {
        Object result = bundle.LoadAsset<Object>(path);
        if (result == null)
            throw new ArgumentException(path + " is not valid in the assetbundle!");

        return result;
    }
}