using System;
using System.Diagnostics.CodeAnalysis;
using Nightfall.Interfaces;

namespace Nightfall;

public static class GrabbableObjectExtensions
{
    public static bool TryGetGrabbableObjectReplacement(this GrabbableObject? grabbableObject, [NotNullWhen(true)] out NightfallItemReplacementDefinition? output)
    {
        output = null;
        if (grabbableObject == null)
        {
            return false;
        }

        output = ((ICurrentEntityReplacement)grabbableObject).CurrentEntityReplacement as NightfallItemReplacementDefinition;
        return output != null;
    }

    [Obsolete($"Use {nameof(TryGetGrabbableObjectReplacement)}")]
    public static NightfallItemReplacementDefinition? GetGrabbableObjectReplacement(this GrabbableObject grabbableObject)
    {
        grabbableObject.TryGetGrabbableObjectReplacement(out NightfallItemReplacementDefinition? output);
        return output;
    }

    internal static void SetGrabbableObjectReplacement(this GrabbableObject grabbableObject, NightfallItemReplacementDefinition itemReplacementDefinition)
    {
        ((ICurrentEntityReplacement)grabbableObject).CurrentEntityReplacement = itemReplacementDefinition;
    }
}
