using System.Collections.Generic;
using System.Linq;
using Dawn;
using Dawn.Internal;
using Nightfall.Interfaces;
using Nightfall.Internal;
using UnityEngine;

namespace Nightfall;

public class NightfallMapObject : MonoBehaviour, ICurrentEntityReplacement
{
    public object? CurrentEntityReplacement { get; set; }


    public NightfallMapObjectReplacementDefinition? GetMapObjectReplacement()
    {
        NightfallMapObjectReplacementDefinition? mapObjectReplacementDefinition = (NightfallMapObjectReplacementDefinition?)CurrentEntityReplacement;
        return mapObjectReplacementDefinition;
    }

    internal bool HasMapObjectReplacement()
    {
        return GetMapObjectReplacement() != null;
    }

    internal void SetMapObjectReplacement(NightfallMapObjectReplacementDefinition mapObjectReplacementDefinition)
    {
        CurrentEntityReplacement = mapObjectReplacementDefinition;
    }

    public void Start()
    {
        DawnMapObjectNamespacedKeyContainer? container = GetComponent<DawnMapObjectNamespacedKeyContainer>();
        if (container == null)
        {
            NightfallPlugin.Logger.LogWarning($"NightfallMapObject: {gameObject.name} doesn't have a DawnMapObjectNamespacedKeyContainer component, this means that you cannot replace this map object.");
            return;
        }

        if (container.Value == null)
        {
            NightfallPlugin.Logger.LogWarning($"Failed to replace MapObject entity for '{container.gameObject.name}', it doesn't have a dawn info! (there may be other problems)");
            return;
        }

        if (!LethalContent.MapObjects[container.Value.AsTyped<DawnMapObjectInfo>()].CustomData.TryGet(NightfallKeys.EntityReplacements, out List<NightfallMapObjectReplacementDefinition>? replacements))
        {
            return;
        }

        if (HasMapObjectReplacement())
        {
            return;
        }

        List<NightfallMapObjectReplacementDefinition> newReplacements = new List<NightfallMapObjectReplacementDefinition>(replacements);
        for (int i = newReplacements.Count - 1; i >= 0; i--)
        {
            NightfallMapObjectReplacementDefinition replacement = newReplacements[i];
            if (replacement.DatePredicate == null)
                continue;

            if (!replacement.DatePredicate.Evaluate())
            {
                newReplacements.RemoveAt(i);
            }
        }

        int totalWeight = newReplacements.Sum(it => it.GetRarity());
        if (EntityReplacementRegistrationPatch.mapObjectReplacementRandom == null)
        {
            EntityReplacementRegistrationPatch.mapObjectReplacementRandom = new System.Random(StartOfRound.Instance.randomMapSeed + 234780);
        }

        int chosenWeight = EntityReplacementRegistrationPatch.mapObjectReplacementRandom.Next(0, totalWeight);
        foreach (NightfallMapObjectReplacementDefinition replacement in newReplacements)
        {
            chosenWeight -= replacement.GetRarity();
            if (chosenWeight > 0)
                continue;

            StartOfRoundRefs.Instance.StartCoroutine(replacement.Apply(this));
            break;
        }
    }

    public void OnDestroy() { }
}