using System;
using UnityEngine;

namespace CodeRebirthLib.ContentManagement;

[Serializable]
public abstract class EntityData
{
    public abstract string EntityName { get; }

    [Obsolete("Use EntityName instead")]
    [HideInInspector]
    [SerializeField]
    public string entityName;
}