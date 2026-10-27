using System.Collections.Generic;
using System.Linq;
using Dawn;
using Dawn.Internal;
using Nightfall.Interfaces;
using Nightfall.Internal;
using Newtonsoft.Json.Linq;
using UnityEngine;
using Dawn.Interfaces;

namespace Nightfall;

public class NightfallUnlockable : MonoBehaviour, ICurrentEntityReplacement, IDawnSaveData
{
    public object? CurrentEntityReplacement { get; set; }

    public AutoParentToShip AutoParentToShip { get; private set; }
    public PlaceableShipObject PlaceableShipObject { get; private set; }

    public NightfallUnlockableReplacementDefinition? GetUnlockableReplacement()
    {
        NightfallUnlockableReplacementDefinition? unlockableReplacementDefinition = (NightfallUnlockableReplacementDefinition?)CurrentEntityReplacement;
        return unlockableReplacementDefinition;
    }

    internal bool HasUnlockableReplacement()
    {
        return GetUnlockableReplacement() != null;
    }

    internal void SetUnlockableReplacement(NightfallUnlockableReplacementDefinition unlockableReplacementDefinition)
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
        Debuggers.Unlockables?.Log($"{this.gameObject.name} nightfall unlockable starting...");
        if (PlaceableShipObject == null)
        {
            return;
        }
        UnlockableItem unlockableItem = StartOfRoundRefs.Instance.unlockablesList.unlockables[PlaceableShipObject.unlockableID];
        if (!unlockableItem.spawnPrefab && unlockableItem.prefabObject == null)
        {
            NightfallPlugin.Logger.LogWarning($"Unlockable: {unlockableItem.unlockableName} doesn't have a prefab nor does it spawn as one, this means that you cannot replace this unlockable.");
            return;
        }

        if (unlockableItem.DawnInfo == null)
        {
            NightfallPlugin.Logger.LogWarning($"Failed to replace unlockable entity for '{unlockableItem.unlockableName}', it doesn't have a dawn info! (there may be other problems)");
            return;
        }

        if (!unlockableItem.DawnInfo.CustomData.TryGet(NightfallKeys.EntityReplacements, out List<NightfallUnlockableReplacementDefinition>? replacements))
        {
            Debuggers.Unlockables?.Log($"Unlockable {unlockableItem.unlockableName} has no replacements!");
            return;
        }

        if (HasUnlockableReplacement())
        {
            return;
        }

        List<NightfallUnlockableReplacementDefinition> newReplacements = new(replacements);
        for (int i = newReplacements.Count - 1; i >= 0; i--)
        {
            NightfallUnlockableReplacementDefinition replacement = newReplacements[i];
            if (replacement.DatePredicate == null)
                continue;

            if (!replacement.DatePredicate.Evaluate())
            {
                newReplacements.RemoveAt(i);
            }
        }

        int totalWeight = newReplacements.Sum(it => it.GetRarity());
        if (EntityReplacementRegistrationPatch.unlockableReplacementRandom == null)
        {
            EntityReplacementRegistrationPatch.unlockableReplacementRandom = new System.Random(StartOfRoundRefs.Instance.randomMapSeed + 234780);
        }

        int chosenWeight = EntityReplacementRegistrationPatch.unlockableReplacementRandom.Next(0, totalWeight);
        foreach (NightfallUnlockableReplacementDefinition replacement in newReplacements)
        {
            chosenWeight -= replacement.GetRarity();
            if (chosenWeight > 0)
                continue;

            StartOfRoundRefs.Instance.StartCoroutine(replacement.Apply(this));
            break;
        }
    }

    public void OnDestroy() { }

    public virtual JToken GetDawnDataToSave()
    {
        return JToken.FromObject(0);
    }

    public virtual void LoadDawnSaveData(JToken saveData)
    {

    }
}