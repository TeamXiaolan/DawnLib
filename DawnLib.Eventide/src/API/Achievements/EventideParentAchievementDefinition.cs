using System.Collections.Generic;
using Dawn.Utils;
using Dusk;
using UnityEngine;
using UnityEngine.Serialization;

namespace Eventide;

[CreateAssetMenu(fileName = "New Parent Achievement Definition", menuName = $"{EventideModConstants.Achievements}/Parent Definition")]
public class EventideParentAchievement : EventideAchievementDefinition, IProgress
{
    [field: SerializeReference]
    [field: FormerlySerializedAs("ChildrenAchievementNames")]
    public List<EventideAchievementReference> ChildrenAchievementReferences { get; private set; } = new();

    public override void Register(DuskRegistrationContext registrationContext)
    {
        base.Register(registrationContext);
        EventideAchievementHandler.OnAchievementUnlocked += definition =>
        {
            if (definition.registrationContext != registrationContext)
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
        foreach (EventideAchievementDefinition achievement in EventideModContent.Achievements)
        {
            if (!achievement.Completed)
                continue;

            foreach (EventideAchievementReference achievementReference in ChildrenAchievementReferences)
            {
                if (achievementReference.TryResolve(out EventideAchievementDefinition achievementDefinition) && achievementDefinition.AchievementName == achievement.AchievementName)
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
        foreach (EventideAchievementReference achievementReference in ChildrenAchievementReferences)
        {
            if (achievementReference.TryResolve(out EventideAchievementDefinition achievementDefinition) && achievementDefinition.IsActive())
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