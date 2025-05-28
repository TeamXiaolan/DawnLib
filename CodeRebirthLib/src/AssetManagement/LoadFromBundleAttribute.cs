using System;

namespace CodeRebirthLib.AssetManagement;

[AttributeUsage(AttributeTargets.Property)]
public class LoadFromBundleAttribute(string bundleFile) : Attribute
{
    public string BundleFile { get; private set; } = bundleFile;
}
