using Dawn.Interfaces;

namespace Dusk;

public static class GrabbableObjectExtensions
{
    public static DuskItemReplacementDefinition? GetGrabbableObjectReplacement(this GrabbableObject grabbableObject)
    {
        DuskItemReplacementDefinition? grabbableObjectReplacementDefinition = (DuskItemReplacementDefinition?)((ICurrentEntityReplacement)grabbableObject).CurrentEntityReplacement;
        return grabbableObjectReplacementDefinition;
    }

    internal static bool HasGrabbableObjectReplacement(this GrabbableObject grabbableObject)
    {
        return grabbableObject.GetGrabbableObjectReplacement() != null;
    }

    internal static void SetGrabbableObjectReplacement(this GrabbableObject grabbableObject, DuskItemReplacementDefinition itemReplacementDefinition)
    {
        ((ICurrentEntityReplacement)grabbableObject).CurrentEntityReplacement = itemReplacementDefinition;
    }
}
