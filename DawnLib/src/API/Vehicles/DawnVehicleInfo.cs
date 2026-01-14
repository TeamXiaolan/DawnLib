using System.Collections.Generic;
using UnityEngine;

namespace Dawn;

public sealed class DawnVehicleInfo : DawnBaseInfo<DawnVehicleInfo>
{
    internal DawnVehicleInfo(NamespacedKey<DawnVehicleInfo> key, HashSet<NamespacedKey> tags, GameObject vehiclePrefab, GameObject? secondaryPrefab, GameObject? stationPrefab, DawnPurchaseInfo dawnPurchaseInfo, IDataContainer? customData) : base(key, tags, customData)
    {
        VehiclePrefab = vehiclePrefab;
        SecondaryPrefab = secondaryPrefab;
        StationPrefab = stationPrefab;
        DawnPurchaseInfo = dawnPurchaseInfo;
    }

    public DawnPurchaseInfo DawnPurchaseInfo { get; }

    public GameObject VehiclePrefab { get; }
    public GameObject? SecondaryPrefab { get; }
    public GameObject? StationPrefab { get; }

    public TerminalNode BuyNode { get; internal set; }
    public TerminalKeyword BuyKeyword { get; internal set; }
    public TerminalNode ConfirmPurchaseNode { get; internal set; }
    public TerminalNode? InfoNode { get; internal set; }
}