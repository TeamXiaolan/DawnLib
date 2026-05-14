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
    public Terrain? Terrain { get; private set; }
    public float[,,] TerrainAlphamaps { get; private set; } = new float[0, 0, 0];
    public List<int> TerrainIndices { get; private set; } = new();

    public void Start()
    {
        if (TryGetComponent(out Terrain terrain))
        {
            TerrainData terrainData = terrain.terrainData;
            TerrainAlphamaps = terrainData.GetAlphamaps(0, 0, terrainData.alphamapWidth, terrainData.alphamapHeight);
            Terrain = terrain;
        }

        foreach (NamespacedKey key in NamespacedKeysForTerrain)
        {
            if (LethalContent.Surfaces.TryGetValue(key, out DawnSurfaceInfo terrainSurfaceInfo))
            {
                TerrainIndices.Add(terrainSurfaceInfo.SurfaceIndex);
            }
        }

        if (NamespacedKey == null || string.IsNullOrEmpty(NamespacedKey.Namespace) || string.IsNullOrEmpty(NamespacedKey.Key) || !LethalContent.Surfaces.TryGetValue(NamespacedKey, out DawnSurfaceInfo surfaceInfo))
        {
            if (Terrain == null)
            {
                DawnPlugin.Logger.LogWarning($"Surface: '{NamespacedKey}' on '{gameObject.name}' on not found.");
            }
            return;
        }

        if (surfaceInfo.Surface == null)
        {
            if (Terrain == null)
            {
                DawnPlugin.Logger.LogWarning($"Surface: '{NamespacedKey}' on '{gameObject.name}' has no footstep surface defined.");
            }
            return;
        }

        SurfaceIndex = surfaceInfo.SurfaceIndex;
    }

    public bool TryGetFootstepIndex(Vector3 point, bool checkStandingOnTerrain, out int footstepIndex, PlayerControllerB? playerControllerB = null)
    {
        footstepIndex = -1;

        if (Terrain != null)
        {
            if (playerControllerB != null)
            {
                playerControllerB.standingOnTerrain = true;
            }

            if (checkStandingOnTerrain)
            {
                return false;
            }

            StartOfRound.Instance.currentTerrainAlphaMaps = TerrainAlphamaps;
            StartOfRound.Instance.gotCurrentTerrainAlphamaps = true;

            Vector3 splatMapCoordinate = ConvertToSplatMapCoordinate(point, Terrain);

            int dominantTextureIndex = 0;
            float highestBlendWeight = 0f;

            TerrainData terrainData = Terrain.terrainData;
            int textureLayerCount = TerrainAlphamaps.Length / (terrainData.alphamapWidth * terrainData.alphamapHeight);

            for (int layerIndex = 0; layerIndex < textureLayerCount; layerIndex++)
            {
                float layerWeight = TerrainAlphamaps[
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