
using Newtonsoft.Json.Linq;

namespace Dawn;

public static class GrabbableObjectExtensions
{
    public static JToken GetSaveData(this GrabbableObject grabbableObject)
    {
        if (!grabbableObject.itemProperties.saveItemVariable)
            return 0;

        if (grabbableObject is DawnGrabbableObject dawnGrabbableObject)
        {
            return dawnGrabbableObject.GetDawnItemDataToSave();
        }
        return grabbableObject.GetItemDataToSave();
    }

    public static void LoadSaveData(this GrabbableObject grabbableObject, JToken jToken)
    {
        if (!grabbableObject.itemProperties.saveItemVariable)
            return;

        if (grabbableObject is DawnGrabbableObject dawnGrabbableObject)
        {
            dawnGrabbableObject.LoadDawnItemSaveData(jToken);
            return;
        }
        grabbableObject.LoadItemSaveData((int)jToken);
    }
}
