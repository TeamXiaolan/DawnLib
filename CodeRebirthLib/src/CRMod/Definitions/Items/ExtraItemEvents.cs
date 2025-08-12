using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace CodeRebirthLib;
[RequireComponent(typeof(GrabbableObject))]
public class ExtraItemEvents : MonoBehaviour
{
    internal static Dictionary<GrabbableObject, ExtraItemEvents> eventListeners = [];

    [SerializeField]
    internal UnityEvent onCollectInShip;

    private void OnEnable()
    {
        eventListeners[GetComponent<GrabbableObject>()] = this;
    }

    private void OnDisable()
    {
        eventListeners.Remove(GetComponent<GrabbableObject>());
    }
}