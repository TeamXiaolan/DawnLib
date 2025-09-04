using System.Collections.Generic;
using Dawn.Utils;
using UnityEngine;
using UnityEngine.Serialization;

namespace Dusk;

[CreateAssetMenu(fileName = "New Parent Achievement Definition", menuName = $"{DuskModConstants.Achievements}/Parent Definition")]
public class DuskParentAchievement : DuskAchievementDefinition, IProgress
{
    [field: SerializeReference]
    [field: FormerlySerializedAs("ChildrenAchievementNames")]
    public List<DuskAchievementReference> ChildrenAchievementReferences { get; private set; } = new();

    public override void Register(DuskMod mod)
    {
        base.Register(mod);
        DuskAchievementHandler.OnAchievementUnlocked += definition =>
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
        foreach (var achievement in DuskModContent.Achievements.Values)
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
        foreach (var achievement in DuskModContent.Achievements.Values)
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