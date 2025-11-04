using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Dusk;

[Serializable]
public class BuyableShip
{
    [field: SerializeField]
    public string ShipName { get; private set; }
    [field: SerializeField]
    public GameObject ShipPrefab { get; private set; }
    [field: SerializeField]
    public GameObject NavmeshPrefab { get; private set; }
    [field: SerializeField]
    public int Cost { get; private set; }
}

