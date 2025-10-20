using Unity.Netcode;

namespace Dawn.Utils;
public abstract class NetworkSingleton<T> : NetworkBehaviour where T : NetworkSingleton<T>
{
    public static T? Instance { get; private set; }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        if (Instance == (T)this) Instance = null;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        Instance = (T)this;
    }
}