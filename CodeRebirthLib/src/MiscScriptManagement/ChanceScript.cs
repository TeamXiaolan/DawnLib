using System.Collections;
using CodeRebirthLib.Util;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

namespace CodeRebirthLib.MiscScriptManagement;

[DefaultExecutionOrder(-999)]
public class ChanceScript : NetworkBehaviour
{
    [SerializeField]
    private UnityEvent _onChance = new();
    [SerializeField]
    [Range(0, 100)]
    private int _chance = 50;

    private NetworkVariable<int> result = new(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (CodeRebirthLibNetworker.Instance == null)
        {
            CodeRebirthLibPlugin.Logger.LogWarning($"CodeRebirthLibNetworker.Instance is null! I really hope you're starting up the round right now");
            StartCoroutine(DelayRandomThing());
            return;
        }
        if (IsServer)
        {
            int randomNumber = UnityEngine.Random.Range(0, 100) + 1;
            result.Value = randomNumber;
        }

        if (result.Value > _chance)
            return;

        _onChance.Invoke();
    }

    private IEnumerator DelayRandomThing()
    {
        yield return new WaitUntil(() => CodeRebirthLibNetworker.Instance != null);
        if (IsServer)
        {
            int randomNumber = UnityEngine.Random.Range(0, 100) + 1;
            result.Value = randomNumber;
        }

        if (result.Value > _chance)
            yield break;

        _onChance.Invoke();
    }
}
