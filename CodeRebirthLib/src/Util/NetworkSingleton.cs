using System;
using Unity.Netcode;

namespace CodeRebirthLib.Util;
public abstract class NetworkSingleton<T> : NetworkBehaviour where T : NetworkSingleton<T>
{
    public static T Instance { get; private set; }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        Instance = (T)this;
    }

    protected void OnDisable()
    {
        if (Instance == (T)this) Instance = null;
    }
}