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

    public BepInPlugin CreatePluginMetadata()
    {
        return new BepInPlugin(AuthorName + ModName, ModName, Version);
    }
}