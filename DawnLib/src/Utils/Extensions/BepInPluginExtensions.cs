using BepInEx;
using Dawn.Internal;

namespace Dawn.Utils;
public static class BepInPluginExtensions
{
    public static DataContainer GetPersistentDataContainer(this BaseUnityPlugin plugin)
    {
        return PersistentDataHandler.Get(plugin);
    }
}