using Unity.Netcode;
using UnityEngine;

namespace CodeRebirthLib;

public sealed class CRMapObjectInfo : CRBaseInfo<CRMapObjectInfo>
{
    internal CRMapObjectInfo(NamespacedKey<CRMapObjectInfo> key, bool isExternal, GameObject mapObject, CRInsideMapObjectInfo? insideInfo, CROutsideMapObjectInfo? outsideInfo) : base(key, isExternal)
    {
        MapObject = mapObject;
        InsideInfo = insideInfo;
        if (InsideInfo != null) InsideInfo.ParentInfo = this;
        OutsideInfo = outsideInfo;
        if (OutsideInfo != null) OutsideInfo.ParentInfo = this;
        HasNetworkObject = mapObject.GetComponent<NetworkObject>() != null;
    }
    
    public GameObject MapObject { get; }
    public CRInsideMapObjectInfo? InsideInfo { get; }
    public CROutsideMapObjectInfo? OutsideInfo { get; }
    public bool HasNetworkObject { get; }
}