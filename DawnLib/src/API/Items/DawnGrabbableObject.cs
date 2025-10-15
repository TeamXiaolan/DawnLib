using Newtonsoft.Json.Linq;

namespace Dawn;

public abstract class DawnGrabbableObject : GrabbableObject
{
    public virtual JToken GetDawnItemDataToSave()
    {
        return GetItemDataToSave();
    }

    public virtual void LoadDawnItemSaveData(JToken saveData)
    {
        if (saveData.Type == JTokenType.Integer)
        {
            LoadItemSaveData((int)saveData);
        }
    }
}