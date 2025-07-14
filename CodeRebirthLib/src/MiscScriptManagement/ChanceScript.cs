using System.Collections;
using CodeRebirthLib.Util;
using UnityEngine;
using UnityEngine.Events;

namespace CodeRebirthLib.MiscScriptManagement;

[DefaultExecutionOrder(-999)]
public class ChanceScript : MonoBehaviour
{
    [SerializeField]
    private UnityEvent _onChance = new();
    [SerializeField]
    [Range(0, 100)]
    private int _chance = 50;

    public void Awake()
    {
        if (CodeRebirthLibNetworker.Instance == null)
        {
            CodeRebirthLibPlugin.Logger.LogWarning($"CodeRebirthLibNetworker.Instance is null! I really hope you're starting up the round right now");
            StartCoroutine(DelayRandomThing());
            return;
        }
        int randomNumber = CodeRebirthLibNetworker.Instance.CRLibRandom.Next(0, 100) + 1;
        if (randomNumber > _chance)
            return;

        _onChance.Invoke();
    }
    
    private IEnumerator DelayRandomThing()
    {
        yield return new WaitUntil(() => CodeRebirthLibNetworker.Instance != null);
        int randomNumber = CodeRebirthLibNetworker.Instance!.CRLibRandom.Next(0, 100) + 1;
        if (randomNumber > _chance)
            yield break;

        _onChance.Invoke();
    }
}
