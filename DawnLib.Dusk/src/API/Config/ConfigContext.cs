using System;
using BepInEx.Configuration;
using Dawn.Utils;

namespace Dusk;
public class ConfigContext(ConfigFile file, string heading) : IDisposable
{

    public void Dispose() { }

    public ConfigEntry<T> Bind<T>(string name, string description, T defaultValue = default)
    {
        return file.CleanedBind(heading, name, defaultValue, description);
    }
}