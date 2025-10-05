using System;
using System.Collections.Generic;
using Unity.Netcode;
using XXHash = Unity.Netcode.XXHash;

namespace Dawn.Internal;
static class DawnNetworkSceneManager
{
    private static readonly Dictionary<string, uint> _pathToHash = [];
    private static readonly Dictionary<uint, string> _hashToPath = [];
    private static readonly Dictionary<string, string> _nameToPath = [];

    internal static void AddScenePath(string scenePath)
    {
        string name = GetSceneNameFromPath(scenePath);
        uint hash = scenePath.Hash32();
        _pathToHash[scenePath] = hash;
        _hashToPath[hash] = scenePath;
        _nameToPath[name] = scenePath;
        Debuggers.SceneManager?.Log($"Added new network scene: '{name}' (hash: {hash})");
    }
    
    // this is taken from NetworkSceneManager. i have no idea why it isn't static by default
    static string GetSceneNameFromPath(string scenePath)
    {
        int num = scenePath.LastIndexOf("/", StringComparison.Ordinal) + 1;
        int num2 = scenePath.LastIndexOf(".", StringComparison.Ordinal);
        return scenePath.Substring(num, num2 - num);
    }
    
    internal static void Init()
    {
        On.Unity.Netcode.NetworkSceneManager.GenerateScenesInBuild += LogExtraDebugInformation;
        On.Unity.Netcode.NetworkSceneManager.SceneHashFromNameOrPath += HashFromScene;
        On.Unity.Netcode.NetworkSceneManager.ScenePathFromHash += SceneFromHash;
        
        On.Unity.Netcode.NetworkSceneManager.ValidateSceneEvent += ValidateSceneEvent;
    }
    private static void LogExtraDebugInformation(On.Unity.Netcode.NetworkSceneManager.orig_GenerateScenesInBuild orig, NetworkSceneManager self)
    {
        orig(self);
        Debuggers.SceneManager?.Log($"DawnLibSceneManager has {_nameToPath.Count} scenes.");
    }
    private static string SceneFromHash(On.Unity.Netcode.NetworkSceneManager.orig_ScenePathFromHash orig, NetworkSceneManager self, uint sceneHash)
    {
        if (_hashToPath.TryGetValue(sceneHash, out string scenePath))
        {
            return scenePath;
        }
        return orig(self, sceneHash);
    }
    private static uint HashFromScene(On.Unity.Netcode.NetworkSceneManager.orig_SceneHashFromNameOrPath orig, NetworkSceneManager self, string sceneNameOrPath)
    {
        if (_nameToPath.TryGetValue(sceneNameOrPath, out string scenePath))
        {
            return _pathToHash[scenePath];
        }
        if (_pathToHash.TryGetValue(sceneNameOrPath, out uint hash))
        {
            return hash;
        }
        return orig(self, sceneNameOrPath);
    }
    private static object ValidateSceneEvent(On.Unity.Netcode.NetworkSceneManager.orig_ValidateSceneEvent orig, NetworkSceneManager self, string sceneName, bool isUnloading)
    {
        Debuggers.SceneManager?.Log($"On.ValidateSceneEvent: {sceneName}, {isUnloading}");
        if (_nameToPath.TryGetValue(sceneName, out string scenePath))
        {
            Debuggers.SceneManager?.Log($"_nameToPath hit");
            SceneEventProgress sceneEventProgress = new SceneEventProgress(self.NetworkManager, SceneEventProgressStatus.Started)
            {
                SceneHash = _pathToHash[scenePath]
            };
            self.SceneEventProgressTracking.Add(sceneEventProgress.Guid, sceneEventProgress);
            self.m_IsSceneEventActive = true;
            sceneEventProgress.OnComplete = new SceneEventProgress.OnCompletedDelegate(self.OnSceneEventProgressCompleted);
            return sceneEventProgress;
        }
        return orig(self, sceneName, isUnloading);
    }
}