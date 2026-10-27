using Dawn.Internal;
using Newtonsoft.Json.Linq;
using Dawn;

namespace Nightfall.Internal;

internal static class NightfallSaveIntegration
{
    private static readonly string EntitySaveKey = "nightfall_entity_replacement_skin_key";

    internal static void Init()
    {
        DawnItemSaveEvents.OnCollectExtraSaveData += OnCollectExtraSaveData;
        DawnItemSaveEvents.OnLoadExtraSaveData += OnLoadExtraSaveData;
    }

    private static void OnCollectExtraSaveData(GrabbableObject item, JObject extraData)
    {
        if (!item.TryGetGrabbableObjectReplacement(out NightfallItemReplacementDefinition? replacement) || replacement == null)
        {
            return;
        }

        JObject skinData = new()
        {
            ["replacementKey"] = replacement.Key.ToString()
        };

        extraData[EntitySaveKey] = skinData;
    }

    private static void OnLoadExtraSaveData(GrabbableObject grabbableObject, JObject extraData)
    {
        if (extraData[EntitySaveKey] is not JObject skinData)
        {
            return;
        }

        string? replacementKeyString = skinData["replacementKey"]?.ToObject<string>();
        if (string.IsNullOrWhiteSpace(replacementKeyString) || !NamespacedKey.TryParse(replacementKeyString, out NamespacedKey? replacementKey))
        {
            return;
        }

        if (!NightfallModContent.EntityReplacements.TryGetValue(replacementKey, out NightfallEntityReplacementDefinition replacement))
        {
            return;
        }

        if (replacement is not NightfallItemReplacementDefinition itemReplacement)
        {
            return;
        }

        StartOfRoundRefs.Instance.StartCoroutine(itemReplacement.Apply(grabbableObject));
    }
}