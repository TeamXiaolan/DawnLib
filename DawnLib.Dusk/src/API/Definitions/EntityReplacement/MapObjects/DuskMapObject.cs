using System.Collections.Generic;
using System.Linq;
using Dawn;
using Dawn.Internal;
using Dawn.Interfaces;
using Dusk.Internal;
using UnityEngine;

namespace Dusk;

public class DuskMapObject : MonoBehaviour, ICurrentEntityReplacement
{
    public object? CurrentEntityReplacement { get; set; }


    public DuskMapObjectReplacementDefinition? GetMapObjectReplacement()
    {
        DuskMapObjectReplacementDefinition? mapObjectReplacementDefinition = (DuskMapObjectReplacementDefinition?)CurrentEntityReplacement;
        return mapObjectReplacementDefinition;
    }

    internal bool HasMapObjectReplacement()
    {
        return GetMapObjectReplacement() != null;
    }

    internal void SetMapObjectReplacement(DuskMapObjectReplacementDefinition mapObjectReplacementDefinition)
    {
        CurrentEntityReplacement = mapObjectReplacementDefinition;
    }

    public void Start()
    {
        DawnMapObjectNamespacedKeyContainer? container = GetComponent<DawnMapObjectNamespacedKeyContainer>();
        if (container == null)
        {
            DuskPlugin.Logger.LogWarning($"DuskMapObject: {gameObject.name} doesn't have a DawnMapObjectNamespacedKeyContainer component, this means that you cannot replace this map object.");
            return;
        }

        if (container.Value == null)
        {
            DuskPlugin.Logger.LogWarning($"Failed to replace MapObject entity for '{container.gameObject.name}', it doesn't have a dawn info! (there may be other problems)");
            return;
        }

        if (!LethalContent.MapObjects[container.Value.AsTyped<DawnMapObjectInfo>()].CustomData.TryGet(DuskKeys.EntityReplacements, out List<DuskMapObjectReplacementDefinition>? replacements))
        {
            return;
        }

        if (HasMapObjectReplacement())
        {
            return;
        }

        List<DuskMapObjectReplacementDefinition> newReplacements = new List<DuskMapObjectReplacementDefinition>(replacements);
        for (int i = newReplacements.Count - 1; i >= 0; i--)
        {
            DuskMapObjectReplacementDefinition replacement = newReplacements[i];
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
        foreach (DuskMapObjectReplacementDefinition replacement in newReplacements)
        {
            chosenWeight -= replacement.GetRarity();
            if (chosenWeight > 0)
                continue;

            if (replacement.IsDefault)
                break;

            StartOfRoundRefs.Instance.StartCoroutine(replacement.Apply(this));
            break;
        }
    }

    public void OnDestroy() { }
}