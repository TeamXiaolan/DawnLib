using System.Linq;
using Dawn;
using Dawn.Internal;
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
    public NamespacedKey<DuskVehicleDefinition> VehicleKey { get; private set; }
    [field: SerializeField]
    public NamespacedKey CorrespondingStationKey { get; private set; }

    [field: SerializeField]
    public Transform[] RopeAttachmentEndPoints { get; private set; }

    NamespacedKey IVehicle.VehicleKey => VehicleKey;

    protected bool InDropShipAnimation { get; private set; } = false;
    public int RealLength { get; private set; } = 4;
    public Collider[] VehicleColliders { get; private set; }

    public virtual void Awake()
    {
        if (RopeAttachmentEndPoints.Length > 4)
        {
            RopeAttachmentEndPoints = RopeAttachmentEndPoints.Take(4).ToArray();
        }
        else if (RopeAttachmentEndPoints.Length < 4)
        {
            RealLength = RopeAttachmentEndPoints.Length;
            for (int i = RopeAttachmentEndPoints.Length; i < 4; i++)
            {
                RopeAttachmentEndPoints[i] = new GameObject($"RopeAttachmentPoint{i + 1}").transform;
                RopeAttachmentEndPoints[i].SetParent(this.transform, false);
                RopeAttachmentEndPoints[i].position = this.transform.position;
            }
        }

        VehicleColliders = GetComponentsInChildren<Collider>();
        int vehicleLayer = LayerMask.NameToLayer("Vehicle");
        foreach (VehicleBase vehicleBase in FindObjectsOfType<VehicleBase>())
        {
            if (vehicleBase == this)
                continue;

            foreach (Collider collider1 in vehicleBase.VehicleColliders)
            {
                if (collider1.isTrigger || collider1.gameObject.layer != vehicleLayer)
                    continue;

                foreach (Collider collider2 in VehicleColliders)
                {
                    if (collider2.isTrigger || collider2.gameObject.layer != vehicleLayer)
                        continue;

                    Physics.IgnoreCollision(collider1, collider2);
                }
            }
        }

        InDropShipAnimation = !StartOfRoundRefs.Instance.inShipPhase && TerminalRefs.LastVehicleDelivered == DuskModContent.Vehicles[VehicleKey].DawnVehicleInfo.BuyNode.buyVehicleIndex && ItemDropshipRefs.Instance.deliveringVehicle && !ItemDropshipRefs.Instance.untetheredVehicle;
    }

    public virtual void Update()
    {
    }

    public virtual void LateUpdate()
    {
        if (!StartOfRoundRefs.Instance.inShipPhase && TerminalRefs.LastVehicleDelivered == DuskModContent.Vehicles[VehicleKey].DawnVehicleInfo.BuyNode.buyVehicleIndex && ItemDropshipRefs.Instance.deliveringVehicle && !ItemDropshipRefs.Instance.untetheredVehicle)
        {
            for (int i = RealLength; i < 4; i++)
            {
                RopeAttachmentEndPoints[i].position = ItemDropshipRefs.Instance.ropes[i].transform.position;
            }
        }
    }

    public virtual void FixedUpdate()
    {
        if (InDropShipAnimation)
        {
            this.transform.position = ItemDropshipRefs.Instance.deliverVehiclePoint.position;
            InDropShipAnimation = !StartOfRoundRefs.Instance.inShipPhase && TerminalRefs.LastVehicleDelivered == DuskModContent.Vehicles[VehicleKey].DawnVehicleInfo.BuyNode.buyVehicleIndex && ItemDropshipRefs.Instance.deliveringVehicle && !ItemDropshipRefs.Instance.untetheredVehicle;
        }
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