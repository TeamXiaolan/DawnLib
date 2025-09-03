using System;
using UnityEngine;

namespace Dawn;
public class BundleException(AssetBundle bundle, string message) : Exception(message)
{
    public AssetBundle Bundle { get; } = bundle;
}