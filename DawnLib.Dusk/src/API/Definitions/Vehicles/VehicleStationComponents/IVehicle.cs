using Dawn;
using UnityEngine;
using UnityEngine.Events;

namespace Dusk;

public interface IVehicle : IDropShipAttachment
{
    NamespacedKey VehicleKey { get; }
    bool IsDocked { get; }
    IStation? CurrentStation { get; }
    UnityEvent<IStation> Docked { get; }
    UnityEvent<IStation> Undocked { get; }
    Collider[] VehicleColliders { get; }

    void RequestDock(IStation? station);
    void RequestUndock(IStation? station);
    void OnDocked(IStation station);
    void OnUndocked(IStation station);
}