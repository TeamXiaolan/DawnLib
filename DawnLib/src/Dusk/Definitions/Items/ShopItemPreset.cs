using System;
using UnityEngine;

namespace Dawn.Dusk;

[Serializable]
public class ShopItemPreset
{
    [field: SerializeField]
    public TerminalNode OrderRequestNode { get; private set; }

    [field: SerializeField]
    public TerminalNode OrderReceiptNode { get; private set; }

    [field: SerializeField]
    public TerminalNode ItemInfoNode { get; private set; }
}