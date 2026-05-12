using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace Dawn;

public sealed class DawnMapObjectInfo : DawnBaseInfo<DawnMapObjectInfo>
{
    internal DawnMapObjectInfo(NamespacedKey<DawnMapObjectInfo> key, HashSet<NamespacedKey> tags, DawnInsideMapObjectInfo? insideInfo, DawnOutsideMapObjectInfo? outsideInfo, IDataContainer? customData) : base(key, tags, customData)
    {
        InsideInfo = insideInfo;
        if (InsideInfo != null)
        {
            InsideInfo.ParentInfo = this;
            HasNetworkObject = InsideInfo.IndoorMapHazardType.prefabToSpawn.GetComponent<NetworkObject>() != null;
        }

        OutsideInfo = outsideInfo;
        if (OutsideInfo != null)
        {
            OutsideInfo.ParentInfo = this;
            HasNetworkObject = OutsideInfo.SpawnableOutsideObject.prefabToSpawn.GetComponent<NetworkObject>() != null;
        }
    }

    public GameObject GetMapObjectPrefab()
    {
        if (InsideInfo != null)
        {
            return InsideInfo.IndoorMapHazardType.prefabToSpawn;
        }

        if (OutsideInfo != null)
        {
            return OutsideInfo.SpawnableOutsideObject.prefabToSpawn;
        }

        DawnPlugin.Logger.LogError($"Failed to get map object prefab for {Key}");
        return null!; // This should probably throw
    }

    public DawnInsideMapObjectInfo? InsideInfo { get; private set; }
    public DawnOutsideMapObjectInfo? OutsideInfo { get; private set; }
    public bool HasNetworkObject { get; }
}