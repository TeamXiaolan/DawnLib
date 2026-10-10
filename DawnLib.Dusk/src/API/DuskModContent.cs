using Dawn;

namespace Dusk;

public static class DuskModContent
{
    public static Registry<DuskVehicleDefinition> Vehicles = new();
    public static Registry<DuskEntityReplacementDefinition> EntityReplacements = new();
    public static Registry<DuskNamespacedObjectDefinition> NamespacedObjects = new();
    public static Registry<DuskConfigDefinition> Configs = new();
}