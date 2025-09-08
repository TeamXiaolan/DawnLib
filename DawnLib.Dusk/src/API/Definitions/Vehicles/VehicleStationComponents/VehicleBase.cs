using Dawn;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

namespace Dusk;

public abstract class VehicleBase : NetworkBehaviour, IVehicle
{
    [field: SerializeField]
    public UnityEvent<IStation> Docked { get; private set; }

    [field: SerializeField]
    public UnityEvent<IStation> Undocked { get; private set; }

    public bool IsDocked { get; private set; }
    public IStation? CurrentStation { get; private set; }

    [field: SerializeField]
    public NamespacedKey VehicleKey { get; private set; }
    [field: SerializeField]
    public NamespacedKey CorrespondingStationKey { get; private set; }

    public virtual void Awake()
    {
    }

    public virtual void RequestDock(IStation? station)
    {
        if (station == null || !station.CanAccept(this) || CorrespondingStationKey != station.StationKey)
            return;

        station.Engage(this);
    }

    public virtual void RequestUndock(IStation? station)
    {
        if (CurrentStation == null || station != CurrentStation)
            return;

        station.Release(this);
    }

    public virtual void OnDocked(IStation station)
    {
        IsDocked = true;
        CurrentStation = station;
        Docked?.Invoke(station);
    }

    public virtual void OnUndocked(IStation station)
    {
        IsDocked = false;
        CurrentStation = null;
        Undocked?.Invoke(station);
    }
}