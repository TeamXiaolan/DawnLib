
namespace Dawn;

public abstract class DawnGrabbableObject : GrabbableObject
{
    public virtual object GetDawnItemDataToSave()
    {
        return GetItemDataToSave();
    }

    public virtual void LoadDawnItemSaveData(object saveData)
    {
        if (saveData is int intSaveData)
        {
            LoadItemSaveData(intSaveData);
        }
    }
}