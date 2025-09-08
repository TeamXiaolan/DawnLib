using System.Collections.Generic;
using UnityEngine;

namespace Dawn;

public sealed class DawnVehicleInfo : DawnBaseInfo<DawnVehicleInfo>, ITerminalPurchase
{
    internal DawnVehicleInfo(ITerminalPurchasePredicate predicate, NamespacedKey<DawnVehicleInfo> key, HashSet<NamespacedKey> tags, GameObject vehiclePrefab, GameObject stationPrefab, IProvider<int> cost, IDataContainer? customData) : base(key, tags, customData)
    {
        PurchasePredicate = predicate;
        VehiclePrefab = vehiclePrefab;
        StationPrefab = stationPrefab;
        Cost = cost;
    }

    public IProvider<int> Cost { get; }
    public ITerminalPurchasePredicate PurchasePredicate { get; }

    public GameObject VehiclePrefab { get; }
    public GameObject SecondaryPrefab { get; }
    public GameObject? StationPrefab { get; }
}