using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

namespace Dawn.Utils;

[AddComponentMenu($"{DawnConstants.MiscUtils}/Spawn Synced Map Object")]
public class SpawnSyncedDawnLibObject : MonoBehaviour
{
    [Range(0f, 100f)]
    public float chanceOfSpawningAny = 100f;
    public bool automaticallyAlignWithTerrain = false;
    public List<DawnLibObjectTypeWithRarity> objectTypesWithRarity = new();

    public void Start()
    {
        if (!NetworkManager.Singleton.IsServer)
            return;

        if (UnityEngine.Random.Range(0, 100) >= chanceOfSpawningAny)
            return;

        List<(GameObject objectType, float weight)> spawnableObjectsList = new();
        foreach (var objectTypeWithRarity in objectTypesWithRarity)
        {
            if (LethalContent.MapObjects.TryGetValue(objectTypeWithRarity.NamespacedMapObjectKey, out DawnMapObjectInfo info))
            {
                spawnableObjectsList.Add((info.MapObject, objectTypeWithRarity.Rarity));
            }
        }

        if (spawnableObjectsList.Count <= 0)
        {
            DawnPlugin.Logger.LogWarning($"No prefabs found for spawning in game object: {this.gameObject.name}");
            return;
        }

        GameObject? prefabToSpawn = DawnLibUtilities.ChooseRandomWeightedType(spawnableObjectsList);

        // Instantiate and spawn the object on the network.
        if (prefabToSpawn == null)
        {
            DawnPlugin.Logger.LogError($"Did you really set something to spawn at a weight of 0? Couldn't find prefab for spawning: {string.Join(", ", objectTypesWithRarity.Select(objectType => objectType.NamespacedMapObjectKey))}");
            return;
        }

        if (automaticallyAlignWithTerrain)
        {
            if (Physics.Raycast(transform.position + Vector3.up * 2f, Vector3.down, out RaycastHit hit, 100f, StartOfRound.Instance.collidersAndRoomMaskAndDefault, QueryTriggerInteraction.Ignore))
            {
                transform.position = hit.point;
                transform.up = hit.normal;
            }
        }

        var spawnedObject = Instantiate(prefabToSpawn, transform.position, transform.rotation, transform);
        spawnedObject.GetComponent<NetworkObject>().Spawn(true);
    }
}