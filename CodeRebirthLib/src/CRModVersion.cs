using BepInEx;
using UnityEngine;

namespace CodeRebirthLib;
[CreateAssetMenu(fileName = "Mod Information", menuName = "CodeRebirthLib/Mod Information", order = -11)]
public class CRModVersion : ScriptableObject
{
    [field: SerializeField]
    public string Name { get; private set; }
    
    [field: SerializeField]
    public string Version { get; private set; }

    public BepInPlugin CreatePluginMetadata()
    {
        return new BepInPlugin(Name, Name, Version);
    }
}