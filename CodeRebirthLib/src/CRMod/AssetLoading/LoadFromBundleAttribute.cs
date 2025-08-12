using System;

namespace CodeRebirthLib.CRMod;
[AttributeUsage(AttributeTargets.Property)]
public class LoadFromBundleAttribute(string bundleFile) : Attribute
{
    public string BundleFile { get; private set; } = bundleFile;
}