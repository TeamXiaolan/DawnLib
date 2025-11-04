using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Dusk.src.API.Definitions.Ship.Progressive
{
    [AddComponentMenu($"{DuskModConstants.ProgressiveComponents}/Ship Upgrade Scrap")]
    internal class ShipUpgradeScrap : GrabbableObject
    {
        [field: SerializeReference]
        public DuskShipReference UnlockableReference { get; private set; } = null!;
    }
}
