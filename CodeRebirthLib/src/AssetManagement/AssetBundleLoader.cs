using System;
using System.IO;
using System.Reflection;
using CodeRebirthLib.ContentManagement;
using LethalLib.Modules;
using Unity.Netcode;
using UnityEngine;
using NetworkPrefabs = LethalLib.Modules.NetworkPrefabs;

namespace CodeRebirthLib.AssetManagement;
public class AssetBundleLoader<T> where T : AssetBundleLoader<T>
{
    public AssetBundleData? AssetBundleData { get; set; } = null;
    public CRContentDefinition[] Content { get; private set; }
    
    protected AssetBundleLoader(CRMod mod, string filePath) : this(mod.Assembly, filePath)
    { }

    internal AssetBundleLoader(Assembly assembly, string filePath) : this(CodeRebirthLib.LoadBundle(assembly, filePath))
    { }

    internal AssetBundleLoader(AssetBundle bundle)
    {
        Type type = typeof(T);
        foreach (PropertyInfo property in type.GetProperties())
        {
            LoadFromBundleAttribute loadInstruction = (LoadFromBundleAttribute)property.GetCustomAttribute(typeof(LoadFromBundleAttribute));
            if (loadInstruction == null) continue;

            property.SetValue(this, LoadAsset(bundle, loadInstruction.BundleFile));
        }
        foreach (GameObject gameObject in bundle.LoadAllAssets<GameObject>())
        {
            Utilities.FixMixerGroups(gameObject);
            CodeRebirthLibPlugin.ExtendedLogging($"[AssetBundle Loading] Fixed Mixer Groups: {gameObject.name}");

            if (gameObject.GetComponent<NetworkObject>() == null)
                continue; 

            NetworkPrefabs.RegisterNetworkPrefab(gameObject);
            CodeRebirthLibPlugin.ExtendedLogging($"[AssetBundle Loading] Registered Network Prefab: {gameObject.name}");
        }

        Content = bundle.LoadAllAssets<CRContentDefinition>();
    }
    
    private UnityEngine.Object LoadAsset(AssetBundle bundle, string path)
    {
        UnityEngine.Object result = bundle.LoadAsset<UnityEngine.Object>(path);
        if (result == null)
            throw new ArgumentException(path + " is not valid in the assetbundle!");

        return result;
    }
}