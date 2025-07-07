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
        int randomNumber = CodeRebirthLibNetworker.Instance!.CRLibRandom.Next(0, 100) + 1;
        if (randomNumber > _chance)
            return;

        _onChance.Invoke();
    }
}
