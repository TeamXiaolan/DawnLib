using UnityEngine;

namespace Dawn.Utils;
public abstract class Singleton<T> : MonoBehaviour where T : Singleton<T>
{
    private static T? _instance;

    public static T? Instance
    {
        get
        {
            if (_instance == null)
            {
                /*DawnPlugin.Logger.LogWarning($"Tried to get instance reference to {typeof(T).Name} singleton, but it isn't created yet.");
                DawnPlugin.Logger.LogWarning("There will likely be issues!");*/
            }
            return _instance;
        }
    }

    protected virtual void OnDestroy()
    {
        if (Instance == (T)this) _instance = null;
    }

    public void Awake()
    {
        _instance = (T)this;
    }
}