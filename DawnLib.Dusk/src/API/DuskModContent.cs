using Dawn;
using Dusk.src.API.Definitions.Ship;

namespace Dusk;

public static class DuskModContent
{
    public static Registry<DuskAchievementDefinition> Achievements = new();
    public static Registry<DuskShipDefinition> Ships = new();
    public static Registry<DuskVehicleDefinition> Vehicles = new();
    public static Registry<DuskEntityReplacementDefinition> EntityReplacements = new();
}