using System.Collections.Generic;
using CodeRebirthLib.AssetManagement;
using UnityEngine;
using UnityEngine.Serialization;

namespace CodeRebirthLib.ContentManagement.Enemies;

[CreateAssetMenu(fileName = "New Enemy Definition", menuName = "CodeRebirthLib/Enemy Definition")]
public class CREnemyDefinition : CRContentDefinition
{
    [field: FormerlySerializedAs("enemyType"), SerializeField]
    public EnemyType EnemyType { get; private set; }
    
    [field: FormerlySerializedAs("terminalNode"), SerializeField]
    public TerminalNode? TerminalNode { get; private set; }
    
    [field: FormerlySerializedAs("terminalKeyword"), SerializeField]
    public TerminalKeyword? TerminalKeyword { get; private set; }

    [HideInInspector]
    public Dictionary<string, float> WeatherMultipliers = new();
}