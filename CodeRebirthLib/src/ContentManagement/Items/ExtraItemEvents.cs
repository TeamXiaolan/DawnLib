using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace CodeRebirthLib.ContentManagement.Items;
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