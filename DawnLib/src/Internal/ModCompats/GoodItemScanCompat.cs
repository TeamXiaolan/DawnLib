using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using BepInEx.Bootstrap;
using GoodItemScan;
using UnityEngine;

namespace Dawn.Internal;

static class GoodItemScanCompat
{
    private static bool? _enabled = null;
    public static bool Enabled
    {
        get
        {
            _enabled ??= Chainloader.PluginInfos.ContainsKey(GoodItemScan.MyPluginInfo.PLUGIN_GUID);
            return (bool)_enabled;
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    public static bool TryGetRectTransform(ScanNodeProperties scanNodeProperties, [NotNullWhen(true)] out RectTransform? rectTransform)
    {
        rectTransform = null;
        if (GoodItemScan.GoodItemScan.scanner == null)
        {
            return false;
        }

        foreach (GoodItemScan.ScannedNode scannedNode in GoodItemScan.GoodItemScan.scanner.activeNodes)
        {
            if (scannedNode.ScanNodeProperties != scanNodeProperties)
            {
                continue;
            }

            rectTransform = scannedNode.RectTransform;
            return true;
        }

        return false;
    }

    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    public static bool TryGetScanNode(RectTransform rectTransform, [NotNullWhen(true)] out ScanNodeProperties? scanNodeProperties)
    {
        scanNodeProperties = null;
        if (GoodItemScan.GoodItemScan.scanner == null)
        {
            return false;
        }

        foreach (ScannedNode scannedNode in GoodItemScan.GoodItemScan.scanner.activeNodes)
        {
            if (rectTransform != scannedNode.RectTransform)
            {
                continue;
            }

            scanNodeProperties = scannedNode.ScanNodeProperties;
            return true;
        }

        return false;
    }
}