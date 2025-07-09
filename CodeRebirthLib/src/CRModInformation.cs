using System.Collections.Generic;
using BepInEx;
using UnityEngine;

namespace CodeRebirthLib;
[CreateAssetMenu(fileName = "Mod Information", menuName = "CodeRebirthLib/Mod Information", order = -11)]
public class CRModInformation : ScriptableObject
{
    [field: SerializeField]
    public string AuthorName { get; private set; }

    [field: SerializeField]
    public string ModName { get; private set; }

    [field: SerializeField]
    public string Version { get; private set; }

    [field: SerializeField]
    public TextAsset READMEFile { get; private set; }

    [field: SerializeField]
    public TextAsset ChangelogFile { get; private set; }

    [field: SerializeField]
    public string ModDescription { get; private set; }

    [field: SerializeField]
    [Tooltip("Comma separated list of dependencies that this mod depends on apart from the default CRLib, BepInEx and potentially WeatherRegistry, grab from the thunderstore page.")]
    public List<string> ExtraDependencies { get; private set; } = new();

    [field: SerializeField]
    public string WebsiteUrl { get; private set; }

    [field: SerializeField]
    public Texture2D ModIcon { get; private set; }

    public BepInPlugin CreatePluginMetadata()
    {
        return new BepInPlugin(AuthorName + "." + ModName, ModName, Version);
    }
}