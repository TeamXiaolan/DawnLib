using System.Collections.Generic;
using UnityEngine;

namespace CodeRebirthLib.ContentManagement;

// TODO: this is awful.
[CreateAssetMenu(fileName = "New Content Container", menuName = "CodeRebirthLib/Content Container")]
public class ContentContainer : ScriptableObject
{
    public List<AssetBundleData> assetBundles;
}