using System.Collections.Generic;
using System.Linq;
using Dawn;
using Dawn.Internal;
using Dawn.Preloader.Interfaces;
using Dusk.Internal;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Dusk;

public class DuskUnlockable : MonoBehaviour, ICurrentEntityReplacement, IDawnSaveData
{
    public object? CurrentEntityReplacement { get; set; }

    public AutoParentToShip AutoParentToShip { get; private set; }
    public PlaceableShipObject PlaceableShipObject { get; private set; }

    public DuskUnlockableReplacementDefinition? GetUnlockableReplacement()
    {
        DuskUnlockableReplacementDefinition? unlockableReplacementDefinition = (DuskUnlockableReplacementDefinition?)CurrentEntityReplacement;
        return unlockableReplacementDefinition;
    }

    internal bool HasUnlockableReplacement()
    {
        return GetUnlockableReplacement() != null;
    }

    internal void SetUnlockableReplacement(DuskUnlockableReplacementDefinition unlockableReplacementDefinition)
    {
        CurrentEntityReplacement = unlockableReplacementDefinition;
    }

    public void Awake()
    {
        AutoParentToShip = GetComponentInChildren<AutoParentToShip>();
        PlaceableShipObject = GetComponentInChildren<PlaceableShipObject>();
    }

    public void Start()
    {
        UnlockableItem unlockableItem = StartOfRoundRefs.Instance.unlockablesList.unlockables[PlaceableShipObject.unlockableID];
        if (!unlockableItem.spawnPrefab && unlockableItem.prefabObject == null)
        {
            DuskPlugin.Logger.LogWarning($"Unlockable: {unlockableItem.unlockableName} doesn't have a prefab nor does it spawn as one, this means that you cannot replace this unlockable.");
            return;
        }

        if (!unlockableItem.HasDawnInfo())
        {
            DuskPlugin.Logger.LogWarning($"Failed to replace unlockable entity for '{unlockableItem.unlockableName}', it doesn't have a dawn info! (there may be other problems)");
            return;
        }

        if (!unlockableItem.GetDawnInfo().CustomData.TryGet(EntityReplacementRegistrationPatch.Key, out List<DuskUnlockableReplacementDefinition>? replacements))
        {
            return;
        }

        if (HasUnlockableReplacement())
        {
            return;
        }

        DawnMoonInfo currentMoon = RoundManager.Instance.currentLevel.GetDawnInfo();

        int? totalWeight = replacements.Sum(it => it.Weights.GetFor(currentMoon));
        if (totalWeight == null)
        {
            return;
        }

        EntityReplacementRegistrationPatch.replacementRandom ??= new System.Random(StartOfRoundRefs.Instance.randomMapSeed + 234780);

        int chosenWeight = EntityReplacementRegistrationPatch.replacementRandom.Next(0, totalWeight.Value);
        foreach (DuskUnlockableReplacementDefinition replacement in replacements)
        {
            chosenWeight -= replacement.Weights.GetFor(currentMoon) ?? 0;
            if (chosenWeight > 0)
                continue;

            if (replacement.IsDefault)
                break;

            replacement.Apply(this);
        }
    }

    public void OnDestroy() { }

    public virtual JToken GetDawnItemDataToSave()
    {
        return 0;
    }

    public virtual void LoadDawnItemSaveData(JToken saveData)
    {
        
    }
}