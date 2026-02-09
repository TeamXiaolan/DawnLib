using System.Collections.Generic;
using UnityEngine;

namespace Dawn;
public class DawnSceneObjectReference : MonoBehaviour
{
    public string sceneObjectReferenceSearch;
    public bool keepVisible = false;

    [Space]
    public Color hologramColor = new Color(.15f, .8f, .8f, .25f);
    public Color wireframeColor = new Color(0f, 1f, 1f, .25f);

    [HideInInspector]
    public string cachedObjectPath = "";

    //[HideInInspector]
    //public List<Mesh> cachedMeshes = new List<Mesh>();

    //[HideInInspector]
    //public List<Matrix4x4> cachedTransforms = new List<Matrix4x4>();
}
