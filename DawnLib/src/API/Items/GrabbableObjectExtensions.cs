
using Dawn.Preloader.Interfaces;
using Newtonsoft.Json.Linq;

namespace Dawn;

public static class GrabbableObjectExtensions
{
    public static JToken GetSaveData(this GrabbableObject grabbableObject)
    {
        if (!grabbableObject.itemProperties.saveItemVariable)
            return 0;

        if (grabbableObject is IDawnSaveData dawnSaveData)
        {
            return dawnSaveData.GetDawnDataToSave();
        }
        return grabbableObject.GetItemDataToSave();
    }

    public static void LoadSaveData(this GrabbableObject grabbableObject, JToken jToken)
    {
        if (!grabbableObject.itemProperties.saveItemVariable)
            return;

        if (grabbableObject is IDawnSaveData dawnSaveData)
        {
            dawnSaveData.LoadDawnSaveData(jToken);
            return;
        }
        grabbableObject.LoadItemSaveData((int)jToken);
    }
}
