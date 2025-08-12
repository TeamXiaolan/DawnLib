using System;
using BepInEx.Configuration;

namespace CodeRebirthLib.CRMod;
public class ConfigContext(ConfigFile file, string heading) : IDisposable
{

    public void Dispose() { }
    public ConfigEntry<T> Bind<T>(string name, string description, T defaultValue = default)
    {
        return file.Bind(ConfigManager.CleanStringForConfig(heading), ConfigManager.CleanStringForConfig(name), defaultValue, description);
    }
}