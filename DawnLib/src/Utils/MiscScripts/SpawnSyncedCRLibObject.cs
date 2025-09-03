using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

namespace Dawn.Utils;
public class SpawnSyncedCRLibObject : MonoBehaviour
{
    [Range(0f, 100f)]
    public float chanceOfSpawningAny = 100f;
    public bool automaticallyAlignWithTerrain = false;
    public List<CRLibObjectTypeWithRarity> objectTypesWithRarity = new();

    public void Start()
    {
        if (!NetworkManager.Singleton.IsServer)
            return;

        if (UnityEngine.Random.Range(0, 100) >= chanceOfSpawningAny)
            return;

        List<(GameObject objectType, float weight)> spawnableObjectsList = new();
        foreach (var objectTypeWithRarity in objectTypesWithRarity)
        {
            foreach (var mapObjectInfo in LethalContent.MapObjects.Values)
            {
                if (!objectTypeWithRarity.NamespacedMapObjectKey.Equals(mapObjectInfo.Key))
                    continue;

                spawnableObjectsList.Add((mapObjectInfo.MapObject, objectTypeWithRarity.Rarity));
                break;
            }
        }

        if (spawnableObjectsList.Count <= 0)
        {
            CodeRebirthLibPlugin.Logger.LogWarning($"No prefabs found for spawning in game object: {this.gameObject.name}");
            return;
        }

        GameObject? prefabToSpawn = CRLibUtilities.ChooseRandomWeightedType(spawnableObjectsList);

        // Instantiate and spawn the object on the network.
        if (prefabToSpawn == null)
        {
            CodeRebirthLibPlugin.Logger.LogError($"Did you really set something to spawn at a weight of 0? Couldn't find prefab for spawning: {string.Join(", ", objectTypesWithRarity.Select(objectType => objectType.NamespacedMapObjectKey))}");
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