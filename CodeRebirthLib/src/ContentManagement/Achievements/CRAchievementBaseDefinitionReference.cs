using System;
using UnityEngine;

namespace CodeRebirthLib.ContentManagement.Achievements;
[Serializable]
public class CRAchievementBaseDefinitionReference
{
    [SerializeField]
    private string achievementAsset;

    [SerializeField]
    private string achievementName;

    public string AchievementName => achievementName;

    public static implicit operator string(CRAchievementBaseDefinitionReference reference)
    {
        return reference?.AchievementName;
    }
}