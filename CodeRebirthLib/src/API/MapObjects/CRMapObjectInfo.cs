using System.Collections.Generic;
using UnityEngine;

namespace CodeRebirthLib;

public sealed class CRMapObjectInfo : INamespaced<CRMapObjectInfo>
{
    internal CRMapObjectInfo(NamespacedKey<CRMapObjectInfo> key, GameObject mapObject, Dictionary<NamespacedKey<CRMoonInfo>, AnimationCurve> animationCurveToLevelDict, CRInsideMapObjectInfo? insideInfo, CROutsideMapObjectInfo? outsideInfo)
    {
        MapObject = mapObject;
        AnimationCurveToLevelDict = animationCurveToLevelDict;
        InsideInfo = insideInfo;
        if (InsideInfo != null) InsideInfo.ParentInfo = this;
        OutsideInfo = outsideInfo;
        if (OutsideInfo != null) OutsideInfo.ParentInfo = this;
        TypedKey = key;
    }
    
    public GameObject MapObject { get; }
    public Dictionary<NamespacedKey<CRMoonInfo>, AnimationCurve> AnimationCurveToLevelDict { get; } = new();
    public CRInsideMapObjectInfo? InsideInfo { get; }
    public CROutsideMapObjectInfo? OutsideInfo { get; }
    public NamespacedKey Key => TypedKey;
    public NamespacedKey<CRMapObjectInfo> TypedKey { get; }
}