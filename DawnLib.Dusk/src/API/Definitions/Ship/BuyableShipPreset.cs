using System;
using System.Collections.Generic;
using System.Text;
using Unity.Netcode;
using UnityEngine;

namespace Dusk;

[Serializable]
public class BuyableShipPreset
{
    [field: SerializeField]
    public string ShipName { get; private set; }

    [field: SerializeField]
    public GameObject ShipPrefab { get; private set; }
    [field: SerializeField]
    public GameObject NavmeshPrefab { get; private set; }

    [field: SerializeField]
    public List<GameObject> ShipNetworkObjects { get; private set; }

    public int Cost;

    [TextArea(2, 20)]
    public string DisplayNodeText;

    public string BuyKeywordText;
}

