using Dawn.Preloader;
using Newtonsoft.Json.Linq;

namespace Dawn.Interfaces;

[InjectInterface("GrabbableObject")]
public interface IDawnSaveData
{
    public virtual JToken GetDawnDataToSave()
    {
        return ((GrabbableObject)this).GetItemDataToSave();
    }

    public void LoadDawnSaveData(JToken saveData)
    {
        if (saveData.Type != JTokenType.Integer)
            return;

        ((GrabbableObject)this).LoadItemSaveData((int)saveData);
    }
}