using System;
using System.Collections.Generic;
using CodeRebirthLib.ConfigManagement;
using UnityEngine;
using UnityEngine.Serialization;

namespace CodeRebirthLib.AssetManagement;
public class CRContentDefinition : ScriptableObject
{
    [FormerlySerializedAs("ConfigEntries")]
    public List<CRDynamicConfig> _configEntries;

    [HideInInspector]
    public IReadOnlyList<CRDynamicConfig> ConfigEntries => _configEntries;
}