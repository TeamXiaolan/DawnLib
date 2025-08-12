using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace CodeRebirthLib.CRMod;

[CreateAssetMenu(fileName = "New Parent Achievement Definition", menuName = "CodeRebirthLib/Definitions/Achievements/Parent Definition")]
public class CRParentAchievement : CRAchievementDefinition, IProgress
{
    [field: SerializeReference] [field: FormerlySerializedAs("ChildrenAchievementNames")]
    public List<CRAchievementReference> ChildrenAchievementReferences { get; private set; } = new();

    public override void Register(CRMod mod)
    {
        base.Register(mod);
        CRAchievementHandler.OnAchievementUnlocked += definition =>
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
        foreach (var achievement in CRModContent.Achievements.Values)
        {
            if (!achievement.Completed)
                continue;

            foreach (var achievementReference in ChildrenAchievementReferences)
            {
                if (achievementReference.Resolve().AchievementName == achievement.AchievementName)
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
        foreach (var achievement in CRModContent.Achievements.Values)
        {
            if (achievement == this)
                continue;

            if (!achievement.IsActive())
                continue;

            foreach (var achievementReference in ChildrenAchievementReferences)
            {
                if (achievementReference.Resolve().AchievementName == achievement.AchievementName)
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