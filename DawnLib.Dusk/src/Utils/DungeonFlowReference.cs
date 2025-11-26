using System;
using UnityEngine;

namespace Dusk.Utils;

[Serializable]
public class DungeonFlowReference
{
    [field: SerializeField]
    private string _flowAssetName;

    [field: SerializeField]
    private string _bundleName;

    public string FlowAssetName => _flowAssetName;
    public string BundleName => _bundleName;

    public static implicit operator string(DungeonFlowReference reference)
    {
        return reference.FlowAssetName;
    }
}