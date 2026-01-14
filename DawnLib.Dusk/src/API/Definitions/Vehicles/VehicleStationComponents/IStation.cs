using Dawn;
using UnityEngine;

namespace Dusk;
public interface IStation : IAnchorModule
{
    NamespacedKey StationKey { get; }
    Transform AnchorPoint { get; }
    bool IsOccupied { get; }
    IVehicle? CurrentVehicle { get; }

    bool CanAccept(IVehicle? vehicle);
    bool Engage(IVehicle? vehicle);
    bool Release(IVehicle? vehicle);
}