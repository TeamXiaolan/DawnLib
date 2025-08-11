using Unity.Netcode;
using UnityEngine;

namespace CodeRebirthLib;

public sealed class CRMapObjectInfo : INamespaced<CRMapObjectInfo>
{
    internal CRMapObjectInfo(NamespacedKey<CRMapObjectInfo> key, GameObject mapObject, CRInsideMapObjectInfo? insideInfo, CROutsideMapObjectInfo? outsideInfo)
    {
        MapObject = mapObject;
        InsideInfo = insideInfo;
        if (InsideInfo != null) InsideInfo.ParentInfo = this;
        OutsideInfo = outsideInfo;
        if (OutsideInfo != null) OutsideInfo.ParentInfo = this;
        TypedKey = key;
        HasNetworkObject = mapObject.GetComponent<NetworkObject>() != null;
    }
    
    public GameObject MapObject { get; }
    public CRInsideMapObjectInfo? InsideInfo { get; }
    public CROutsideMapObjectInfo? OutsideInfo { get; }
    public bool HasNetworkObject { get; }
    public NamespacedKey Key => TypedKey;
    public NamespacedKey<CRMapObjectInfo> TypedKey { get; }
}