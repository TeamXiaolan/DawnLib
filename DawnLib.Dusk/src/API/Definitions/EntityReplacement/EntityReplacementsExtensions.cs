using System.Diagnostics.CodeAnalysis;
using Dawn;

namespace Dusk;

public static class EntityReplacementsExtensions
{
    public static bool TryGetReplacementByNetworkId(this Registry<DuskEntityReplacementDefinition> entityReplacementRegistry, ulong networkID, [NotNullWhen(true)] out DuskEntityReplacementDefinition? entityReplacement)
    {
        entityReplacement = null;

        foreach (DuskEntityReplacementDefinition entityReplacementDefinition in entityReplacementRegistry.Values)
        {
            if (entityReplacementDefinition.Key.NetworkID != networkID)
                continue;

            entityReplacement = entityReplacementDefinition;
            return true;
        }

        return false;
    }
}