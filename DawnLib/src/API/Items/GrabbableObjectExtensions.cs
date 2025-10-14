using Dawn.Preloader.Interfaces;
using Newtonsoft.Json.Linq;

namespace Dawn;

public static class GrabbableObjectExtensions
{
    public static JToken? GetSaveData(this GrabbableObject grabbableObject)
    {
        if (!grabbableObject.itemProperties.saveItemVariable)
            return null;

        JToken? JToken = ((IModdedSaveData)grabbableObject).GetDataToSave();
        if (JToken == null)
        {
            return grabbableObject.GetItemDataToSave();
        }
        return JToken;
    }

    public static void LoadSaveData(this GrabbableObject grabbableObject, JToken token)
    {
        if (!grabbableObject.itemProperties.saveItemVariable)
            return;

        ((IModdedSaveData)grabbableObject).LoadSaveData(token);
    }
}
