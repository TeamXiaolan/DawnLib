using System.Collections.Generic;
using CodeRebirthLib.Data;
using UnityEngine;

namespace CodeRebirthLib.ContentManagement.Achievements;

[CreateAssetMenu(fileName = "New Parent Achievement Definition", menuName = "CodeRebirthLib/Definitions/Achievements/Parent Definition")]
public class CRParentAchievement : CRAchievementBaseDefinition, IProgress
{
    [field: SerializeField]
    public List<CRAchievementBaseDefinitionReference> ChildrenAchievementReferences { get; private set; } = new();

    public override void Register(CRMod mod)
    {
        base.Register(mod);
        CRMod.OnAchievementUnlocked += definition =>
        {
            if (definition.Mod != mod)
                return;

            if (CountCompleted() >= ChildrenAchievementReferences.Count)
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

            foreach (var reference in ChildrenAchievementReferences)
            {
                if (reference.AchievementName == achievement.AchievementName)
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
            if (!achievement.IsActive())
                continue;

            foreach (var reference in ChildrenAchievementReferences)
            {
                if (reference.AchievementName == achievement.AchievementName)
                {
                    counter += 1;
                    break;
                }
            }
        }
        return counter == ChildrenAchievementReferences.Count;
    }

    public float MaxProgress => ChildrenAchievementReferences.Count;
    public float CurrentProgress => CountCompleted();
}