using System.Collections.Generic;
using UnityEngine;

namespace Dusk;

[CreateAssetMenu(fileName = "New Content Container", menuName = $"{DuskModConstants.MenuName}/Content Container", order = DuskModConstants.DuskModInfoOrder)]
[HelpURL("https://thunderstore.io/c/lethal-company/p/TeamXiaolan/DawnLib/wiki/4099-b2-registering-via-unity-editor/")]
public class ContentContainer : ScriptableObject
{
    public List<AssetBundleData> assetBundles;
}