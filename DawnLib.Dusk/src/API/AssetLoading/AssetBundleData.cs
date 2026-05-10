using System;

namespace Dusk;

[Serializable]
public class AssetBundleData
{
    [AssetBundleReference]
    public string assetBundleName;

    public bool enabledByDefault = true;

    [AssertNotEmpty]
    public string configName;
}