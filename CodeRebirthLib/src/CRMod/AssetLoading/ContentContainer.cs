using System.Collections.Generic;
using UnityEngine;

namespace CodeRebirthLib.CRMod;

[CreateAssetMenu(fileName = "New Content Container", menuName = "CodeRebirthLib/Content Container", order = -15)]
public class ContentContainer : ScriptableObject
{
    public List<AssetBundleData> assetBundles;
}