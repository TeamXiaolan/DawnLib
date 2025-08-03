using System;
using UnityEngine;

namespace CodeRebirthLib.ContentManagement.Achievements;
[Serializable]
public class CRAchievementBaseDefinitionReference
{
#if UNITY_EDITOR
    [SerializeField]
    private CRAchievementBaseDefinitionAsset achievementAsset;
#endif

    [SerializeField]
    private string achievementName;

    public string AchievementName => AchievementName;

    public static implicit operator string(CRAchievementBaseDefinitionReference reference)
    {
        return reference?.AchievementName;
    }
}