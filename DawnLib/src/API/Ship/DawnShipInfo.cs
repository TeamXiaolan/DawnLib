using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Dawn;
public class DawnShipInfo : DawnBaseInfo<DawnShipInfo>
{
    public string ShipName;
    public GameObject ShipPrefab;
    public GameObject NavmeshPrefab;
    public int Cost;

    public DawnShipInfo(NamespacedKey<DawnShipInfo> key, HashSet<NamespacedKey> tags, string shipName, GameObject shipPrefab, GameObject navmeshPrefab, int cost, IDataContainer? customData) : base(key, tags, customData)
    {
        ShipName = shipName;
        ShipPrefab = shipPrefab;
        NavmeshPrefab = navmeshPrefab;
        Cost = cost;
    }
}
