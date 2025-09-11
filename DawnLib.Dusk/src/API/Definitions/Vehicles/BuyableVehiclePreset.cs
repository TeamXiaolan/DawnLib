using System;
using UnityEngine;

namespace Dusk;

[Serializable]
public class BuyableVehiclePreset
{
    public GameObject VehiclePrefab;

    [Header("Optional")]
    public GameObject? StationPrefab;
    public GameObject? SecondaryPrefab;

    [TextArea(2, 20)]
    public string DisplayNodeText;

    public string BuyKeywordText;

    internal TerminalNode BuyNode;
    internal TerminalKeyword BuyKeyword;
    internal TerminalNode ConfirmPurchaseNode;
    public TerminalNode? InfoNode;
}