using System.Collections.Generic;
using BepInEx;
using UnityEngine;

namespace Dusk;

[CreateAssetMenu(fileName = "New Mod Information", menuName = $"{DuskModConstants.MenuName}/Mod Information", order = DuskModConstants.DuskModInfoOrder)]
public class DuskModInformation : ScriptableObject
{
    [field: SerializeField, AssertNotEmpty]
    public string AuthorName { get; private set; }

    [field: SerializeField, AssertNotEmpty]
    public string ModName { get; private set; }

    [field: SerializeField, AssertNotEmpty]
    public string Version { get; private set; }

    [field: SerializeField]
    public TextAsset? READMEFile { get; private set; }

    [field: SerializeField]
    public TextAsset? ChangelogFile { get; private set; }

    [field: SerializeField, AssertNotEmpty]
    public string ModDescription { get; private set; }

    [field: SerializeField, AssertNotEmpty]
    [Tooltip("Comma separated list of dependencies that this mod depends on apart from the default DawnLib, BepInEx and potentially WeatherRegistry, grab from the thunderstore page.")]
    public List<string> ExtraDependencies { get; private set; } = new();

    [field: SerializeField]
    public string WebsiteUrl { get; private set; }

    [field: SerializeField]
    public Sprite? ModIcon { get; private set; }

    internal void SetInfoDetails(string authorName, string modName, string version, string modDescription, string websiteUrl, List<string> extraDependencies, Sprite? modIcon, TextAsset? readmeFile, TextAsset? changelogFile)
    {
        AuthorName = authorName;
        ModName = modName;
        Version = version;
        ModDescription = modDescription;
        WebsiteUrl = websiteUrl;
        ExtraDependencies = extraDependencies;
        ModIcon = modIcon;
        READMEFile = readmeFile;
        ChangelogFile = changelogFile;
    }

    public BepInPlugin CreatePluginMetadata()
    {
        return new BepInPlugin(AuthorName + "." + ModName, ModName, Version);
    }
}