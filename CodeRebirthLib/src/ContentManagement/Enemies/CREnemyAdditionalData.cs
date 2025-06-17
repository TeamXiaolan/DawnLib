using System;
using System.Collections.Generic;
using GameNetcodeStuff;
using UnityEngine;

namespace CodeRebirthLib.ContentManagement.Enemies;
public class CREnemyAdditionalData : MonoBehaviour
{
    private static readonly Dictionary<EnemyAI, CREnemyAdditionalData> _additionalData = new();
    private EnemyAI _enemy;
    
    public PlayerControllerB? PlayerThatLastHit { get; internal set; }
    public bool KilledByPlayer { get; internal set; }
    
    private void OnDestroy()
    {
        _additionalData.Remove(_enemy);
    }

    public static CREnemyAdditionalData CreateOrGet(EnemyAI enemyAI)
    {
        if (_additionalData.TryGetValue(enemyAI, out CREnemyAdditionalData data))
        {
            return data;
        }

        data = enemyAI.gameObject.AddComponent<CREnemyAdditionalData>();
        data._enemy = enemyAI;
        _additionalData[enemyAI] = data;
        return data;
    }
}