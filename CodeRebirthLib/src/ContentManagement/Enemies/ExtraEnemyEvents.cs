using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace CodeRebirthLib.ContentManagement.Enemies;
[RequireComponent(typeof(EnemyAI))]
public class ExtraEnemyEvents : MonoBehaviour
{
    internal static Dictionary<EnemyAI, ExtraEnemyEvents> eventListeners = [];

    [SerializeField]
    internal UnityEvent onKilled, onKilledByPlayer;

    private void OnEnable()
    {
        eventListeners[GetComponent<EnemyAI>()] = this;
    }

    private void OnDisable()
    {
        eventListeners.Remove(GetComponent<EnemyAI>());
    }
}