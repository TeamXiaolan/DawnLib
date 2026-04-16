using Dawn.Internal;
using Newtonsoft.Json.Linq;
using Dawn;
using Unity.Netcode;

namespace Dusk.Internal;

internal static class DuskSaveIntegration
{
    private static readonly string EntitySaveKey = "dusk_entity_replacement_skin_key";

    internal static void Init()
    {
        DawnItemSaveEvents.OnCollectExtraSaveData += OnCollectExtraSaveData;
        DawnItemSaveEvents.OnLoadExtraSaveData += OnLoadExtraSaveData;
    }

    private static void OnCollectExtraSaveData(GrabbableObject item, JObject extraData)
    {
        if (!item.TryGetGrabbableObjectReplacement(out DuskItemReplacementDefinition? replacement) || replacement == null)
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

        if (!DuskModContent.EntityReplacements.TryGetValue(replacementKey, out DuskEntityReplacementDefinition replacement))
        {
            return;
        }

        if (replacement is not DuskItemReplacementDefinition itemReplacement)
        {
            return;
        }

        StartOfRoundRefs.Instance.StartCoroutine(itemReplacement.Apply(grabbableObject));
    }
}