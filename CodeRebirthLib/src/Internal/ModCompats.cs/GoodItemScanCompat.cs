using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using BepInEx.Bootstrap;
using UnityEngine;

namespace CodeRebirthLib.Internal.ModCompats.cs;

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

        if (GoodItemScan.GoodItemScan.scanner._scanNodes.TryGetValue(scanNodeProperties, out int index))
        {
            rectTransform = GoodItemScan.GoodItemScan.scanner._scannedNodes[index].rectTransform;
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

        foreach ((ScanNodeProperties scanNode, int index) in GoodItemScan.GoodItemScan.scanner._scanNodes)
        {
            GoodItemScan.ScannedNode scannedNode = GoodItemScan.GoodItemScan.scanner._scannedNodes[index];
            if (rectTransform != scannedNode.rectTransform)
            {
                continue;
            }

            scanNodeProperties = scanNode;
            return true;
        }

        return false;
    }
}