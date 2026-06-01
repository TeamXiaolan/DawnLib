using System;
using System.Collections.Generic;

namespace Dusk;

[Serializable]
public class AssetBundleData
{
    [AssetBundleReference]
    public string assetBundleName;

    [AssetBundleReference]
    public List<string> DependencyBundleNames = new();

    public bool enabledByDefault = true;

    [AssertNotEmpty]
    public string configName;
}