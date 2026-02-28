using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Dawn;
public class DawnSceneObjectReference : MonoBehaviour
{
    public string sceneObjectReferenceSearch;
    public bool keepVisible = false;

    //color prob should be in editor script
    [Space]
    public Color hologramColor = new Color(.15f, .8f, .8f, .25f);
    public Color wireframeColor = new Color(0f, 1f, 1f, .25f);

    [HideInInspector]
    public string cachedObjectPath = "";

    public Transform? foundObject;

    public bool TryFind()
    {
        //i could assume that root is always Environment but i was thinking about more general aproach 
        //var root = StartOfRound.Instance.shipAnimatorObject.transform.parent;
        if (foundObject) return true;

        var activeScene = SceneManager.GetActiveScene();
        var roots = activeScene.GetRootGameObjects();
        var root = roots.FirstOrDefault(go => go.name == cachedObjectPath.Split('/')[0]);

        if (root)
        {
            var index = cachedObjectPath.IndexOf('/');
            var pathFromRoot = cachedObjectPath.Substring(index + 1);
            foundObject = root.transform.Find(pathFromRoot);
        }

        return foundObject != null;
    }

    public bool TryMove()
    {
        if (foundObject == null) return false;

        foundObject.position = transform.position;
        foundObject.rotation = transform.rotation;
        //foundObject.localScale = transform.localScale;
        //foundObject.parent = transform;

        return true;
    }
}
