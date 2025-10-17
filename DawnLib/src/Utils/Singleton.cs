using UnityEngine;

namespace Dawn.Utils;
public abstract class Singleton<T> : MonoBehaviour where T : Singleton<T>
{
    public static T? Instance { get; private set; }

    protected void OnDestroy()
    {
        if (Instance == (T)this) Instance = null;
    }

    public void Awake()
    {
        Instance = (T)this;
    }
}