using BepInEx.Configuration;

namespace CodeRebirthLib.ContentManagement;

// i hate all of this
public abstract class ContentHandler<T> where T : ContentHandler<T>
{
    public static T Instance { get; private set; } = null!;

    public abstract ConfigFile GetConfigFile();
    
    public ContentHandler()
    {
        Instance = (T)this;
    }
}