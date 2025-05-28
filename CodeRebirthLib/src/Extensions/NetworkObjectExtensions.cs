using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

namespace CodeRebirthLib.Extensions;
public static class NetworkObjectExtensions
{
    public static IEnumerator WaitUntilSpawned(this NetworkObject networkObject)
    {
        yield return new WaitUntil(() => networkObject.IsSpawned);
    }

    static IEnumerator RunActionAfterSpawned(NetworkObject networkObject, Action action)
    {
        yield return networkObject.WaitUntilSpawned();
        action();
    }

    public static void OnSpawn(this NetworkObject networkObject, Action action)
    {
        networkObject.StartCoroutine(RunActionAfterSpawned(networkObject, action));
    }
}