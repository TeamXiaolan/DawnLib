using System.Collections.Generic;
using CodeRebirthLib.AssetManagement;
using UnityEngine;

namespace CodeRebirthLib.ContentManagement;

[CreateAssetMenu(fileName = "New Content Container", menuName = "CodeRebirthLib/Content Container", order = -15)]
public class ContentContainer : ScriptableObject
{
    public List<AssetBundleData> assetBundles;
}