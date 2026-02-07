using Unity.Netcode;

namespace Dawn.Utils;
public abstract class NetworkSingleton<T> : NetworkBehaviour where T : NetworkSingleton<T>
{
    private static T? _instance;

    public static T? Instance
    {
        get
        {
            if (_instance == null)
            {
                /*DawnPlugin.Logger.LogWarning($"Tried to get instance reference to {typeof(T).Name} networksingleton, but it isn't created yet.");
                DawnPlugin.Logger.LogWarning("There will likely be issues!");*/
            }
            return _instance;
        }
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        if (Instance == (T)this) _instance = null;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        _instance = (T)this;
    }
}