using System.Collections.Generic;
using System.Linq;
using Dawn;
using Dawn.Internal;
using Dawn.Interfaces;
using Dusk.Internal;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Dusk;

public class DuskMapObject : MonoBehaviour, ICurrentEntityReplacement, IDawnSaveData
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
        DawnMapObjectInfoContainer? container = GetComponent<DawnMapObjectInfoContainer>();
        if (container == null)
        {
            DuskPlugin.Logger.LogWarning($"DuskMapObject: {gameObject.name} doesn't have a DawnMapObjectInfoContainer component, this means that you cannot replace this map object.");
            return;
        }

        if (container.Value == null)
        {
            DuskPlugin.Logger.LogWarning($"Failed to replace MapObject entity for '{container.gameObject.name}', it doesn't have a dawn info! (there may be other problems)");
            return;
        }

        if (!container.Value.CustomData.TryGet(EntityReplacementRegistrationPatch.Key, out List<DuskMapObjectReplacementDefinition>? replacements))
        {
            return;
        }

        if (HasMapObjectReplacement())
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
        foreach (DuskMapObjectReplacementDefinition replacement in replacements)
        {
            chosenWeight -= replacement.Weights.GetFor(currentMoon) ?? 0;
            if (chosenWeight > 0)
                continue;

            if (replacement.IsDefault)
                break;

            StartOfRoundRefs.Instance.StartCoroutine(replacement.Apply(this));
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