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
        foreach (DuskAchievementDefinition achievement in DuskModContent.Achievements.Values)
        {
            if (!achievement.Completed)
                continue;

            foreach (DuskAchievementReference achievementReference in ChildrenAchievementReferences)
            {
                if (achievementReference.TryResolve(out DuskAchievementDefinition achievementDefinition) && achievementDefinition.AchievementName == achievement.AchievementName)
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
        foreach (DuskAchievementReference achievementReference in ChildrenAchievementReferences)
        {
            if (achievementReference.TryResolve(out DuskAchievementDefinition achievementDefinition) && achievementDefinition.IsActive())
            {
                counter += 1;
            }
        }
        return counter == ChildrenAchievementReferences.Count;
    }

    public float MaxProgress => ChildrenAchievementReferences.Count;
    public float CurrentProgress => CountCompleted();

    public override void TryNetworkRegisterAssets() { }
}