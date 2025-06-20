using Unity.Netcode;

namespace CodeRebirthLib.Util;
public abstract class NetworkSingleton<T> : NetworkBehaviour where T : NetworkSingleton<T>
{
    public static T? Instance { get; private set; }

    protected void OnDisable()
    {
        if (Instance == (T)this) Instance = null;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        Instance = (T)this;
    }
}