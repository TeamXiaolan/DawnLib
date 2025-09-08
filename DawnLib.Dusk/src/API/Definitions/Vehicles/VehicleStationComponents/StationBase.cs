using System.Collections;
using System.Diagnostics.CodeAnalysis;
using Dawn;
using Unity.Netcode;
using UnityEngine;

namespace Dusk;

[RequireComponent(typeof(AutoParentToShip))]
[RequireComponent(typeof(PlaceableShipObject))]
public abstract class StationBase : NetworkBehaviour, IStation
{
    [field: SerializeField]
    public Transform AnchorPoint { get; private set; }

    public bool IsOccupied => CurrentVehicle != null;
    public IVehicle? CurrentVehicle { get; private set; }

    [field: SerializeField]
    public NamespacedKey StationKey { get; private set; }
    [field: SerializeField]
    public NamespacedKey CorrespondingVehicleKey { get; private set; }

    public virtual void Awake()
    {
    }

    public virtual bool CanAccept([NotNullWhen(true)] IVehicle? vehicle)
    {
        if (vehicle == null || vehicle.IsDocked)
            return false;

        if (CorrespondingVehicleKey != vehicle.VehicleKey)
            return false;

        if (IsOccupied)
            return false;

        return true;
    }

    public abstract IEnumerator DisableAnchor(IVehicle vehicle, IStation station);
    public abstract IEnumerator EnableAnchor(IVehicle vehicle, IStation station);

    public virtual bool Engage(IVehicle? vehicle)
    {
        if (!CanAccept(vehicle))
            return false;

        CurrentVehicle = vehicle;
        vehicle.OnDocked(this);
        StartCoroutine(EnableAnchor(vehicle, this));
        return true;
    }

    public virtual bool Release(IVehicle? vehicle)
    {
        if (vehicle == null || vehicle != CurrentVehicle)
            return false;

        if (!IsOccupied)
            return false;

        CurrentVehicle = null;
        vehicle.OnUndocked(this);
        StartCoroutine(DisableAnchor(vehicle, this));
        return true;
    }
}
