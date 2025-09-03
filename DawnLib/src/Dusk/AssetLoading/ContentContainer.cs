using System.Collections.Generic;
using UnityEngine;

namespace Dawn.Dusk;

[CreateAssetMenu(fileName = "New Content Container", menuName = $"{DuskModConstants.MenuName}/Content Container", order = DuskModConstants.DuskModInfoOrder)]
public class ContentContainer : ScriptableObject
{
    public List<AssetBundleData> assetBundles;
}