using System.Collections.Generic;
using UnityEngine;

namespace CodeRebirthLib.CRMod;

[CreateAssetMenu(fileName = "New Parent Achievement Definition", menuName = "CodeRebirthLib/Definitions/Achievements/Parent Definition")]
public class CRParentAchievement : CRAchievementDefinition, IProgress
{
    [field: SerializeReference]
    public List<CRAchievementReference> ChildrenAchievementNames { get; private set; } = new();

    public override void Register(CRMod mod)
    {
        base.Register(mod);
        CRMod.OnAchievementUnlocked += definition =>
        {
            if (definition.Mod != mod)
                return;

            if (CountCompleted() >= ChildrenAchievementNames.Count)
            {
                TryCompleteAchievement();
            }
        };
    }

    int CountCompleted()
    {
        int counter = 0;
        foreach (var achievement in Mod.AchievementRegistry())
        {
            if (!achievement.Completed)
                continue;

            foreach (var achievementName in ChildrenAchievementNames)
            {
                if (achievementName == achievement.AchievementName)
                {
                    counter += 1;
                    break;
                }
            }
        }
        return counter;
    }

    public override bool IsActive()
    {
        int counter = 0;
        foreach (var achievement in Mod.AchievementRegistry())
        {
            if (achievement == this)
                continue;

            if (!achievement.IsActive())
                continue;

            foreach (var achievementName in ChildrenAchievementNames)
            {
                if (achievementName == achievement.AchievementName)
                {
                    counter += 1;
                    break;
                }
            }
        }
        return counter == ChildrenAchievementNames.Count;
    }

    public float MaxProgress => ChildrenAchievementNames.Count;
    public float CurrentProgress => CountCompleted();
}