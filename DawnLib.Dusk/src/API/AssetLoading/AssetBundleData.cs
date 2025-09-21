using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Dusk;

[Serializable]
public class AssetBundleData
{
    [AssetBundleReference]
    public string assetBundleName;

    [field: SerializeField]
    [field: FormerlySerializedAs("keepLoaded")]
    public bool AlwaysKeepLoaded { get; private set; }

    [AssertNotEmpty]
    public string configName;
}