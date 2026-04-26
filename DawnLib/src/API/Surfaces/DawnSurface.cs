using System.Collections.Generic;
using GameNetcodeStuff;
using UnityEngine;

namespace Dawn.Utils;

[RequireComponent(typeof(Collider))]
[AddComponentMenu($"{DawnConstants.MenuName}/Dawn Surface")]
public class DawnSurface : MonoBehaviour
{
    [field: SerializeField]
    [field: InspectorName("Namespace")]
    public NamespacedKey NamespacedKey { get; private set; }

    [field: SerializeField]
    [field: InspectorName("NamespacesForTerrain")]
    [field: Tooltip("Match this list to the list of your terrain's AlphaMasks")]
    public List<NamespacedKey> NamespacedKeysForTerrain { get; private set; } = new();

    [field: SerializeField]
    [field: InspectorName("Center Of Gravity")]
    public GameObject? GravityCenter { get; private set; }

    [field: SerializeField]
    public float GravityStrength { get; private set; } = 1f;

    public int SurfaceIndex { get; private set; } = -1;
    public List<int> TerrainIndices { get; private set; } = new();
    public bool IsTerrain { get; private set; } = false;

    public void Start()
    {
        IsTerrain = this.gameObject.GetComponent<TerrainCollider>() != null;

        foreach (NamespacedKey key in NamespacedKeysForTerrain)
        {
            if (LethalContent.Surfaces.TryGetValue(key, out DawnSurfaceInfo terrainSurfaceInfo))
            {
                TerrainIndices.Add(terrainSurfaceInfo.SurfaceIndex);
            }
        }

        if (NamespacedKey == null || string.IsNullOrEmpty(NamespacedKey.Namespace) || string.IsNullOrEmpty(NamespacedKey.Key) || !LethalContent.Surfaces.TryGetValue(NamespacedKey, out DawnSurfaceInfo surfaceInfo))
        {
            if (!IsTerrain)
            {
                DawnPlugin.Logger.LogWarning($"Surface: '{NamespacedKey}' not found.");
            }
            return;
        }

        if (surfaceInfo.Surface == null)
        {
            if (!IsTerrain)
            {
                DawnPlugin.Logger.LogWarning($"Surface: '{NamespacedKey}' has no footstep surface defined.");
            }
            return;
        }

        SurfaceIndex = surfaceInfo.SurfaceIndex;
    }

    public bool TryGetFootstepIndex(Vector3 point, bool checkStandingOnTerrain, out int footstepIndex, PlayerControllerB? playerControllerB = null)
    {
        footstepIndex = -1;

        if (IsTerrain)
        {
            if (playerControllerB != null)
            {
                playerControllerB.standingOnTerrain = true;
            }

            if (checkStandingOnTerrain)
            {
                return false;
            }

            Terrain activeTerrain = Terrain.activeTerrain;
            TerrainData terrainData = activeTerrain.terrainData;

            if (!StartOfRound.Instance.gotCurrentTerrainAlphamaps)
            {
                StartOfRound.Instance.gotCurrentTerrainAlphamaps = true;
                StartOfRound.Instance.currentTerrainAlphaMaps = terrainData.GetAlphamaps(0, 0, terrainData.alphamapWidth, terrainData.alphamapHeight);
            }

            Vector3 splatMapCoordinate = ConvertToSplatMapCoordinate(point, activeTerrain);

            int dominantTextureIndex = 0;
            float highestBlendWeight = 0f;

            int textureLayerCount = StartOfRound.Instance.currentTerrainAlphaMaps.Length / (terrainData.alphamapWidth * terrainData.alphamapHeight);

            for (int layerIndex = 0; layerIndex < textureLayerCount; layerIndex++)
            {
                float layerWeight = StartOfRound.Instance.currentTerrainAlphaMaps[
                    (int)splatMapCoordinate.z,
                    (int)splatMapCoordinate.x,
                    layerIndex
                ];

                if (highestBlendWeight < layerWeight)
                {
                    highestBlendWeight = layerWeight;
                    dominantTextureIndex = layerIndex;
                }
            }

            if (TerrainIndices.Count <= dominantTextureIndex)
            {
                if (SurfaceIndex == -1)
                {
                    return false;
                }

                footstepIndex = SurfaceIndex;
            }
            else
            {
                footstepIndex = TerrainIndices[dominantTextureIndex];
            }
        }
        else
        {
            footstepIndex = SurfaceIndex;
        }

        return footstepIndex != -1;
    }

    private static Vector3 ConvertToSplatMapCoordinate(Vector3 worldPosition, Terrain terrain)
    {
        Vector3 vector = default;
        Vector3 terrainPosition = terrain.transform.position;

        vector.x = (worldPosition.x - terrainPosition.x) / terrain.terrainData.size.x * terrain.terrainData.alphamapWidth;
        vector.z = (worldPosition.z - terrainPosition.z) / terrain.terrainData.size.z * terrain.terrainData.alphamapHeight;

        return vector;
    }
}