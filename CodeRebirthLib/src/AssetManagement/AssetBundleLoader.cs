using System;
using System.IO;
using System.Reflection;
using LethalLib.Modules;
using Unity.Netcode;
using UnityEngine;
using NetworkPrefabs = LethalLib.Modules.NetworkPrefabs;

namespace CodeRebirthLib.AssetManagement;
public class AssetBundleLoader<T> where T : AssetBundleLoader<T>
{
    protected AssetBundleLoader(string filePath, bool registerNetworkPrefabs = true, bool fixMixerGroups = true)
    {
        string fullPath = Path.Combine(Path.GetDirectoryName(Assembly.GetCallingAssembly().Location)!, "Assets", filePath); // todo: .GetCallingAssembly is probably bad here.
        AssetBundle bundle = AssetBundle.LoadFromFile(fullPath);
        
        Type type = typeof(T);
        foreach (PropertyInfo property in type.GetProperties())
        {
            LoadFromBundleAttribute loadInstruction = (LoadFromBundleAttribute)property.GetCustomAttribute(typeof(LoadFromBundleAttribute));
            if (loadInstruction == null) continue;

            property.SetValue(this, LoadAsset(bundle, loadInstruction.BundleFile));
        }
        
        foreach (GameObject gameObject in bundle.LoadAllAssets<GameObject>())
        {
            if (fixMixerGroups)
            {
                Utilities.FixMixerGroups(gameObject);
                CodeRebirthLibPlugin.ExtendedLogging($"[AssetBundle Loading] Fixed Mixer Groups: {gameObject.name}");
            }

            if (!registerNetworkPrefabs || gameObject.GetComponent<NetworkObject>() == null)
                continue; 

            NetworkPrefabs.RegisterNetworkPrefab(gameObject);
            CodeRebirthLibPlugin.ExtendedLogging($"[AssetBundle Loading] Registered Network Prefab: {gameObject.name}");
        }
    }
    
    private UnityEngine.Object LoadAsset(AssetBundle bundle, string path)
    {
        UnityEngine.Object result = bundle.LoadAsset<UnityEngine.Object>(path);
        if (result == null)
            throw new ArgumentException(path + " is not valid in the assetbundle!");

        return result;
    }
}