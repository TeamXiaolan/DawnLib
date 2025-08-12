using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CodeRebirthLib.Utils;

public static class CRLibUtilities
{
    public static T? ChooseRandomWeightedType<T>(IEnumerable<(T objectType, float rarity)> rarityList)
    {
        // Plugin.ExtendedLogging($"rarityList.Count: {rarityList.Count()}");
        var validObjects = rarityList.Where(x => x.rarity > 0).ToList();

        float cumulativeWeight = 0;
        var cumulativeList = new List<(T?, float)>(validObjects.Count);
        for (int i = 0; i < validObjects.Count; i++)
        {
            cumulativeWeight += validObjects[i].rarity;
            cumulativeList.Add((validObjects[i].objectType, cumulativeWeight));
        }

        // Get a random value in the range [0, cumulativeWeight).
        float randomValue = Random.Range(0, cumulativeWeight);
        T? selectedObject = default(T);

        foreach (var (enemy, cumWeight) in cumulativeList)
        {
            if (randomValue < cumWeight)
            {
                selectedObject = enemy;
                break;
            }
        }

        if (selectedObject == null)
        {
            CodeRebirthLibPlugin.Logger.LogWarning($"Could not find a valid object to spawn of type {typeof(T).Name}!");
            return default;
        }
        return selectedObject;
    }
}