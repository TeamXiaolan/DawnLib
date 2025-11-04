using Dawn;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Dusk;

[CreateAssetMenu(fileName = "New Ship Definition", menuName = $"{DuskModConstants.Definitions}/Ship Definition")]
public class DuskShipDefinition : DuskContentDefinition, INamespaced<DuskShipDefinition>
{
    [field: SerializeField]
    public NamespacedKey<DuskShipDefinition> _typedKey;

    public override NamespacedKey Key { get => TypedKey; protected set => _typedKey = value.AsTyped<DuskShipDefinition>(); }

    public NamespacedKey<DuskShipDefinition> TypedKey => _typedKey;

    protected override string EntityNameReference => Ship.ShipName;

    [field: SerializeField]
    public BuyableShip Ship { get; private set; }

    [field: SerializeField]
    public string InfoNodeText { get; private set; } = string.Empty;

    [field: SerializeField]
    public DuskTerminalPredicate TerminalPredicate { get; private set; }

    [field: SerializeField]
    public DuskPricingStrategy PricingStrategy { get; private set; }

    [field: Space(10)]
    [field: Header("Configs | Main")]

    [field: SerializeField]
    public bool IsInRotation { get; private set; }

    [field: Header("Configs | Misc")]

    [field: SerializeField]
    public bool GenerateDisablePricingStrategyConfig { get; private set; }

    [field: SerializeField]
    public bool GenerateDisableUnlockRequirementConfig { get; private set; }

    public override void Register(DuskMod mod)
    {
        base.Register(mod);
        DuskModContent.Ships.Register(this);
    }


}
