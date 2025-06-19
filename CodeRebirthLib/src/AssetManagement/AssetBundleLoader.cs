using System;
using System.IO;
using System.Reflection;
using CodeRebirthLib.ContentManagement;
using LethalLib.Modules;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Video;
using NetworkPrefabs = LethalLib.Modules.NetworkPrefabs;
using Object = UnityEngine.Object;

namespace CodeRebirthLib.AssetManagement;
public abstract class AssetBundleLoader<T> : IAssetBundleLoader where T : AssetBundleLoader<T>
{
    public AssetBundleData? AssetBundleData { get; set; } = null;
    public CRContentDefinition[] Content { get; private set;  }


    private AssetBundle? _bundle;
    private bool _hasVideoClips, _hasNonPreloadAudioClips;
    
    protected AssetBundleLoader(CRMod mod, string filePath) : this(mod.Assembly, filePath)
    { }

    internal AssetBundleLoader(Assembly assembly, string filePath) : this(CRLib.LoadBundle(assembly, filePath))
    { }

    protected AssetBundleLoader(AssetBundle bundle)
    {
        _bundle = bundle;
        Type type = typeof(T);
        foreach (PropertyInfo property in type.GetProperties())
        {
            LoadFromBundleAttribute loadInstruction = (LoadFromBundleAttribute)property.GetCustomAttribute(typeof(LoadFromBundleAttribute));
            if (loadInstruction == null) continue;

            property.SetValue(this, LoadAsset(bundle, loadInstruction.BundleFile));
        }
        foreach (Object asset in bundle.LoadAllAssets())
        {
            switch(asset) {
                case GameObject gameObject: {
                        Utilities.FixMixerGroups(gameObject);
                        CodeRebirthLibPlugin.ExtendedLogging($"[AssetBundle Loading] Fixed Mixer Groups: {gameObject.name}");

                        if (gameObject.GetComponent<NetworkObject>() == null)
                            continue; 

                        NetworkPrefabs.RegisterNetworkPrefab(gameObject);
                        CodeRebirthLibPlugin.ExtendedLogging($"[AssetBundle Loading] Registered Network Prefab: {gameObject.name}");
                        break;
                    }
                case VideoClip:
                    _hasVideoClips = true;
                    break;
                case AudioClip audioClip:
                    if(audioClip.preloadAudioData) break;
                    
                    _hasNonPreloadAudioClips = true;
                    break;
            }
        }

        Content = bundle.LoadAllAssets<CRContentDefinition>();
    }
    
    internal void TryUnload() {
        if(AssetBundleData?.AlwaysKeepLoaded ?? true) return;

        if (_bundle == null)
        {
            CodeRebirthLibPlugin.Logger.LogWarning("Tried to unload bundle twice?");
        }

        if (_hasVideoClips)
        {
            CodeRebirthLibPlugin.Logger.LogWarning($"Bundle: '{_bundle.name}' has at least one VideoClip but is being unloaded! Playing video clips from this bundle could cause errors! Mark `AlwaysKeepLoaded` as true to stop this from happening.");
        }

        if (_hasNonPreloadAudioClips)
        {
            CodeRebirthLibPlugin.Logger.LogWarning($"Bundle: '{_bundle.name}' is being unloaded but contains an AudioClip that has 'preloadAudioData' to false! This will cause errors when trying to play this clip.");
        }
        
        _bundle.Unload(false);
        _bundle = null;
    }
    
    private UnityEngine.Object LoadAsset(AssetBundle bundle, string path)
    {
        UnityEngine.Object result = bundle.LoadAsset<UnityEngine.Object>(path);
        if (result == null)
            throw new ArgumentException(path + " is not valid in the assetbundle!");

        return result;
    }
}