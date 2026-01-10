using System;
using Dawn.Internal;
using UnityEngine;

namespace Dusk.Utils;

[Serializable]
public class SceneReference
{
    [field: SerializeField]
    private string _assetGUID;
    [field: SerializeField]
    private string _scenePath;
    [field: SerializeField]
    private string _bundleName;

    public string ScenePath => _scenePath;
    public string BundleName => _bundleName;
    public string SceneName => !string.IsNullOrWhiteSpace(ScenePath) ? DawnNetworkSceneManager.GetSceneNameFromPath(ScenePath) : string.Empty;

    public static implicit operator string(SceneReference reference)
    {
        return reference.SceneName;
    }
}