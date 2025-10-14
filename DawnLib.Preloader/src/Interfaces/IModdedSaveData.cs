using Newtonsoft.Json.Linq;

namespace Dawn.Preloader.Interfaces;

[InjectInterface("GrabbableObject")]
public interface IModdedSaveData
{
    JToken? GetDataToSave()
    {
        return null;
    }

    void LoadSaveData(JToken saveData)
    {
        
    }
}