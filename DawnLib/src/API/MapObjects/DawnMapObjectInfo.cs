using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace Dawn;

public sealed class DawnMapObjectInfo : DawnBaseInfo<DawnMapObjectInfo>
{
    internal DawnMapObjectInfo(NamespacedKey<DawnMapObjectInfo> key, HashSet<NamespacedKey> tags, GameObject mapObject, DawnInsideMapObjectInfo? insideInfo, DawnOutsideMapObjectInfo? outsideInfo, IDataContainer? customData) : base(key, tags, customData)
    {
        MapObject = mapObject;
        InsideInfo = insideInfo;
        if (InsideInfo != null) InsideInfo.ParentInfo = this;
        OutsideInfo = outsideInfo;
        if (OutsideInfo != null) OutsideInfo.ParentInfo = this;
        HasNetworkObject = mapObject.GetComponent<NetworkObject>() != null;
    }

    public GameObject MapObject { get; }
    public DawnInsideMapObjectInfo? InsideInfo { get; private set; }
    public DawnOutsideMapObjectInfo? OutsideInfo { get; private set; }
    public bool HasNetworkObject { get; }
}