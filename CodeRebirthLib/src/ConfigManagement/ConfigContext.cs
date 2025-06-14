using System;
using BepInEx.Configuration;

namespace CodeRebirthLib.ConfigManagement;
public class ConfigContext(ConfigFile file, string heading) : IDisposable
{
    public ConfigEntry<T> Bind<T>(string name, string description, T defaultValue = default)
    {
        return file.Bind(ConfigManager.CleanStringForConfig(heading), ConfigManager.CleanStringForConfig(name), defaultValue, description);
    }
    
    public void Dispose() { }
}