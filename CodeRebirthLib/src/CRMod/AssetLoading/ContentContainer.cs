using System.Collections.Generic;
using UnityEngine;

namespace CodeRebirthLib.CRMod;

[CreateAssetMenu(fileName = "New Content Container", menuName = $"{CRModConstants.MenuName}/Content Container", order = CRModConstants.CRModInfoOrder)]
public class ContentContainer : ScriptableObject
{
    public List<AssetBundleData> assetBundles;
}