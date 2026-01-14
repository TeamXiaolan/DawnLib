using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Dawn.Utils;
[RequireComponent(typeof(GrabbableObject))]
[AddComponentMenu($"{DawnConstants.ExtraEvents}/Extra Item Events")]
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