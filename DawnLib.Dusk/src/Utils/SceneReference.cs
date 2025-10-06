using System;
using Dawn.Internal;
using UnityEngine;

namespace Dusk.Utils;

[Serializable]
public class SceneReference
{
    [SerializeField]
    private string _assetGUID;

    [field: SerializeField]
    public string ScenePath { get; private set; }

    public string SceneName => DawnNetworkSceneManager.GetSceneNameFromPath(ScenePath);
}