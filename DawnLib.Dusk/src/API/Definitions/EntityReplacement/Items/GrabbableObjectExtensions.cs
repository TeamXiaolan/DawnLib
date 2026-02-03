using System;
using System.Diagnostics.CodeAnalysis;
using Dawn.Interfaces;

namespace Dusk;

public static class GrabbableObjectExtensions
{
    public static bool TryGetGrabbableObjectReplacement(this GrabbableObject grabbableObject, [NotNullWhen(true)] out DuskItemReplacementDefinition? output)
    {
        output = ((ICurrentEntityReplacement)grabbableObject).CurrentEntityReplacement as DuskItemReplacementDefinition;
        return output != null;
    }

    [Obsolete($"Use {nameof(TryGetGrabbableObjectReplacement)}")]
    public static DuskItemReplacementDefinition? GetGrabbableObjectReplacement(this GrabbableObject grabbableObject)
    {
        grabbableObject.TryGetGrabbableObjectReplacement(out var output);
        return output;
    }

    internal static void SetGrabbableObjectReplacement(this GrabbableObject grabbableObject, DuskItemReplacementDefinition itemReplacementDefinition)
    {
        ((ICurrentEntityReplacement)grabbableObject).CurrentEntityReplacement = itemReplacementDefinition;
    }
}
