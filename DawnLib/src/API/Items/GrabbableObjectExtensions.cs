
namespace Dawn;

public static class GrabbableObjectExtensions
{
    public static object GetSaveData(this GrabbableObject grabbableObject)
    {
        if (!grabbableObject.itemProperties.saveItemVariable)
            return 0;

        if (grabbableObject is DawnGrabbableObject dawnGrabbableObject)
        {
            return dawnGrabbableObject.GetDawnItemDataToSave();
        }
        return grabbableObject.GetItemDataToSave();
    }

    public static void LoadSaveData(this GrabbableObject grabbableObject, object @object)
    {
        if (!grabbableObject.itemProperties.saveItemVariable)
            return;

        if (grabbableObject is DawnGrabbableObject dawnGrabbableObject)
        {
            dawnGrabbableObject.LoadDawnItemSaveData(@object);
            return;
        }
        grabbableObject.LoadItemSaveData((int)@object);
    }
}
