using InjectionLibrary.Attributes;
using Newtonsoft.Json.Linq;

[assembly: RequiresInjections]

namespace Dawn.Interfaces;

[InjectInterface(typeof(GrabbableObject))]
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