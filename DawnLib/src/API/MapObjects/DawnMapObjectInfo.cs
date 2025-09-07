using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace Dawn;

public sealed class DawnMapObjectInfo : DawnBaseInfo<DawnMapObjectInfo>
{
    internal DawnMapObjectInfo(NamespacedKey<DawnMapObjectInfo> key, List<NamespacedKey> tags, GameObject mapObject, DawnInsideMapObjectInfo? insideInfo, DawnOutsideMapObjectInfo? outsideInfo, DataContainer? customData) : base(key, tags, customData)
    {
        MapObject = mapObject;
        InsideInfo = insideInfo;
        if (InsideInfo != null) InsideInfo.ParentInfo = this;
        OutsideInfo = outsideInfo;
        if (OutsideInfo != null) OutsideInfo.ParentInfo = this;
        HasNetworkObject = mapObject.GetComponent<NetworkObject>() != null;
    }

    public GameObject MapObject { get; }
    public DawnInsideMapObjectInfo? InsideInfo { get; }
    public DawnOutsideMapObjectInfo? OutsideInfo { get; }
    public bool HasNetworkObject { get; }
}