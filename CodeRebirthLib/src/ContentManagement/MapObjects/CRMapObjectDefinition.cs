using CodeRebirthLib.AssetManagement;
using UnityEngine;
using UnityEngine.Serialization;

namespace CodeRebirthLib.ContentManagement.MapObjects;

[CreateAssetMenu(fileName = "New Map Definition", menuName = "CodeRebirthLib/Map Definition")]
public class CRMapObjectDefinition : CRContentDefinition
{
    [field: FormerlySerializedAs("gameObject"), SerializeField]
    public GameObject GameObject { get; private set; }
    [field: FormerlySerializedAs("objectName"), SerializeField]
    public string ObjectName { get; private set; }
    [field: FormerlySerializedAs("alignWithTerrain"), SerializeField]
    public bool AlignWithTerrain { get; private set; }
    
    // xu what is this.
    // [field: SerializeField]
    // public SpawnSyncedCRObject.CRObjectType CRObjectType { get; private set; }
}