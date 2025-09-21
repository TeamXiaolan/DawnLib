using Dawn.Preloader.Interfaces;

namespace Dawn;

public static class BuyableVehicleExtensions
{
    public static DawnVehicleInfo GetDawnInfo(this BuyableVehicle vehicle)
    {
        DawnVehicleInfo vehicleInfo = (DawnVehicleInfo)((IDawnObject)vehicle).DawnInfo;
        return vehicleInfo;
    }

    internal static bool HasDawnInfo(this BuyableVehicle vehicle)
    {
        return vehicle.GetDawnInfo() != null;
    }

    internal static void SetDawnInfo(this BuyableVehicle vehicle, DawnVehicleInfo vehicleInfo)
    {
        ((IDawnObject)vehicle).DawnInfo = vehicleInfo;
    }
}
