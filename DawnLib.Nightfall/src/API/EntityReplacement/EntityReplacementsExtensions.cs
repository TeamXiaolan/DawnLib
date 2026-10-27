using System.Diagnostics.CodeAnalysis;
using Dawn;

namespace Nightfall;

public static class EntityReplacementsExtensions
{
    public static bool TryGetReplacementByNetworkId(this Registry<NightfallEntityReplacementDefinition> entityReplacementRegistry, ulong networkID, [NotNullWhen(true)] out NightfallEntityReplacementDefinition? entityReplacement)
    {
        entityReplacement = null;

        foreach (NightfallEntityReplacementDefinition entityReplacementDefinition in entityReplacementRegistry)
        {
            if (entityReplacementDefinition.Key.NetworkID != networkID)
                continue;

            entityReplacement = entityReplacementDefinition;
            return true;
        }

        return false;
    }
}