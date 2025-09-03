using System.Collections.Generic;
using BepInEx;
using UnityEngine;

namespace Dawn.Dusk;
[CreateAssetMenu(fileName = "Mod Information", menuName = $"{CRModConstants.MenuName}/Mod Information", order = CRModConstants.CRModInfoOrder)]
public class CRModInformation : ScriptableObject
{
    [field: SerializeField]
    public string AuthorName { get; internal set; }

    [field: SerializeField]
    public string ModName { get; internal set; }

    [field: SerializeField]
    public string Version { get; internal set; }

    [field: SerializeField]
    public TextAsset? READMEFile { get; private set; }

    [field: SerializeField]
    public TextAsset? ChangelogFile { get; private set; }

    [field: SerializeField]
    public string ModDescription { get; internal set; }

    [field: SerializeField]
    [Tooltip("Comma separated list of dependencies that this mod depends on apart from the default CRLib, BepInEx and potentially WeatherRegistry, grab from the thunderstore page.")]
    public List<string> ExtraDependencies { get; internal set; } = new();

    [field: SerializeField]
    public string WebsiteUrl { get; internal set; }

    [field: SerializeField]
    public Sprite? ModIcon { get; internal set; }

    public BepInPlugin CreatePluginMetadata()
    {
        return new BepInPlugin(AuthorName + "." + ModName, ModName, Version);
    }
}