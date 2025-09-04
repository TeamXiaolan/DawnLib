using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Dawn.Utils;
[RequireComponent(typeof(EnemyAI))]
public class ExtraEnemyEvents : MonoBehaviour
{
    internal static Dictionary<EnemyAI, ExtraEnemyEvents> eventListeners = [];

    [SerializeField]
    internal UnityEvent onKilled, onKilledByPlayer = new();

    private EnemyAI enemyAI = null!;

    private void OnEnable()
    {
        enemyAI = GetComponent<EnemyAI>();
        eventListeners[enemyAI] = this;
    }

    private void OnDisable()
    {
        eventListeners.Remove(enemyAI);
    }
}