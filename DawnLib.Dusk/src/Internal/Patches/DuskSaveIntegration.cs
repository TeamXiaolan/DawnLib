using Dawn.Internal;
using Dawn.Interfaces;
using Newtonsoft.Json.Linq;
using Dawn;

namespace Dusk.Internal;

internal static class DuskItemSaveIntegration
{
    private static readonly NamespacedKey SaveKey = NamespacedKey.From("dusk", "entity_replacement_skin");

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

        extraData[SaveKey] = skinData;
    }

    private static void OnLoadExtraSaveData(GrabbableObject item, JObject extraData)
    {
        if (extraData[SaveKey] is not JObject skinData)
        {
            return;
        }

        NamespacedKey? replacementKey = skinData["replacementKey"]?.ToObject<NamespacedKey>();
        if (replacementKey == null)
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

        ((ICurrentEntityReplacement)item).CurrentEntityReplacement = itemReplacement;
        StartOfRoundRefs.Instance.StartCoroutine(itemReplacement.Apply(item));
    }
}