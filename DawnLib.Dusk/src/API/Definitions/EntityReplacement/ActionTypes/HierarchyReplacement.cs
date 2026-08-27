using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dusk;

public abstract class Hierarchy : ScriptableObject
{
    [field: SerializeField]
    public List<string> HierarchyPaths { get; private set; }

    [field: SerializeField]
    [field: DontDrawIfEmpty]
    public string HierarchyPath { get; private set; }

    public abstract IEnumerator Apply(Transform rootTransform, bool immediate);

    public List<T> GetComponentsWithHierarchyPaths<T>(Transform rootTransform) where T : Component
    {
        if (HierarchyPath == string.Empty && HierarchyPaths == null)
        {
            HierarchyPaths = [string.Empty];
        }
        else if (HierarchyPath != string.Empty)
        {
            if (HierarchyPaths == null)
            {
                HierarchyPaths = [HierarchyPath];
            }
            else
            {
                HierarchyPaths.Add(HierarchyPath);
            }
        }

        List<T> components = new();
        foreach (string hierarchyPath in HierarchyPaths)
        {
            T? component = !string.IsNullOrWhiteSpace(hierarchyPath) ? rootTransform.Find(hierarchyPath).GetComponent<T>() : rootTransform.GetComponent<T>();
            if (component != null)
            {
                components.Add(component);
            }
        }

        return components;
    }
}