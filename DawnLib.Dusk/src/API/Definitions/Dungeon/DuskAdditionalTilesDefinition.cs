using System;
using System.Collections.Generic;
using System.Linq;
using Dawn;
using DunGen;
using Unity.Netcode;
using UnityEngine;

namespace Dusk;

[CreateAssetMenu(fileName = "New Additional Tiles Definition", menuName = $"{DuskModConstants.Definitions}/Additional Tiles Definition")]
public class DuskAdditionalTilesDefinition : DuskContentDefinition<DawnTileSetInfo>
{
    [Flags]
    public enum BranchCapSetting
    {
        Regular = 1 << 0,
        BranchCap = 1 << 1,
    }

    [field: SerializeField]
    public TileSet TilesToAdd { get; private set; }

    [field: SerializeField]
    [field: UnlockedNamespacedKey]
    public List<NamespacedKey<DawnArchetypeInfo>> archetypeKeys = new();

    [field: SerializeField]
    public BranchCapSetting BranchCap { get; private set; } = BranchCapSetting.Regular | BranchCapSetting.BranchCap;

    [field: SerializeField]
    public DuskPredicate? Predicate { get; private set; }

    public override void Register(DuskRegistrationContext registrationContext)
    {
        base.Register(registrationContext);
        DawnTileSetInfo tileSetInfo = DawnLib.DefineTileSet(TypedKey, TilesToAdd, builder =>
        {
            ApplyTagsTo(builder);
            builder.SetIsRegular(BranchCap.HasFlag(BranchCapSetting.Regular));
            builder.SetIsBranchCap(BranchCap.HasFlag(BranchCapSetting.BranchCap));
            if (Predicate != null)
            {
                builder.SetInjectionPredicate(Predicate);
            }
        });

        tileSetInfo.SetupDetails();
        tileSetInfo.RegisterSpawnSyncedObjects();

        LethalContent.Archetypes.BeforeFreezeWithContext += _ =>
        {
            foreach (NamespacedKey<DawnArchetypeInfo> key in archetypeKeys)
            {
                if (LethalContent.Archetypes.TryGetValue(key, out DawnArchetypeInfo archetypeInfo))
                {
                    archetypeInfo.AddTileSet(tileSetInfo);
                }
            }
        };

        LethalContent.Dungeons.OnFreezeWithContext += _ =>
        {
            List<GameObject> potentialPrefabs = new();
            foreach (NetworkPrefab networkPrefab in NetworkManager.Singleton.NetworkConfig.Prefabs.Prefabs)
            {
                potentialPrefabs.Add(networkPrefab.Prefab);
            }

            foreach (SpawnSyncedObject spawnSyncedObject in tileSetInfo.SpawnSyncedObjects)
            {
                if (spawnSyncedObject.spawnPrefab == null)
                {
                    DuskPlugin.Logger.LogWarning("SpawnSyncedObject.spawnPrefab is null in tileSet: " + tileSetInfo.TypedKey);
                    continue;
                }

                if (spawnSyncedObject.spawnPrefab.TryGetComponent(out NetworkObject networkObject))
                {
                    continue;
                }

                GameObject? fixedPrefab = potentialPrefabs.FirstOrDefault(potentialPrefab => potentialPrefab.name == spawnSyncedObject.spawnPrefab.name);
                if (fixedPrefab == null)
                {
                    DuskPlugin.Logger.LogWarning("SpawnSyncedObject's network prefab is missing in tileSet: " + tileSetInfo.TypedKey + ", prefab: " + spawnSyncedObject.spawnPrefab.name + ", spawner: " + spawnSyncedObject.gameObject.name);
                    continue;
                }

                spawnSyncedObject.spawnPrefab = fixedPrefab;
            }
        };
    }

    public override void TryNetworkRegisterAssets()
    {
        foreach (GameObject gameObject in TilesToAdd.TileWeights.Weights.Select(x => x.Value))
        {
            if (!gameObject.TryGetComponent(out NetworkObject _))
                continue;

            DawnLib.RegisterNetworkPrefab(gameObject);
        }
    }
    protected override string EntityNameReference => TilesToAdd?.name ?? string.Empty;
}