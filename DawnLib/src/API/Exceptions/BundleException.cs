using System;
using UnityEngine;

namespace CodeRebirthLib;
public class BundleException(AssetBundle bundle, string message) : Exception(message)
{
    public AssetBundle Bundle { get; } = bundle;
}