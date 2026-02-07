using System.Collections.Generic;
using UnityEngine;

namespace Dawn;
public class DawnSceneObjectReference : MonoBehaviour
{
    public string sceneObjectReferenceSearch;
    public bool searchInInactive = true;
    public bool exactMatch = true;
    public bool keepVisible = false;

    [Space]
    public Color hologramColor = Color.cyan;
    public Color wireframeColor = Color.white;

    [HideInInspector]
    public string cachedObjectPath = "";

    //[HideInInspector]
    //public List<Mesh> cachedMeshes = new List<Mesh>();

    //[HideInInspector]
    //public List<Matrix4x4> cachedTransforms = new List<Matrix4x4>();
}
