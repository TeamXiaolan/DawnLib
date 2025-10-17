using Newtonsoft.Json.Linq;

namespace Dawn.Preloader.Interfaces;

[InjectInterface("GrabbableObject")]
public interface IDawnSaveData
{
    public JToken GetDawnDataToSave()
    {
        return JToken.FromObject(0);;
    }

    public void LoadDawnSaveData(JToken saveData) { }
}