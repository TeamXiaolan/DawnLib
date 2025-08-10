using UnityEngine;

namespace CodeRebirthLib;
public abstract class Singleton<T> : MonoBehaviour where T : Singleton<T>
{
    public static T? Instance { get; private set; }

    protected void OnDisable()
    {
        if (Instance == (T)this) Instance = null;
    }

    public void Awake()
    {
        Instance = (T)this;
    }
}