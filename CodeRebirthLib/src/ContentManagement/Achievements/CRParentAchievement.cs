using System.Collections.Generic;
using System.Linq;
using CodeRebirthLib.Data;
using UnityEngine;

namespace CodeRebirthLib.ContentManagement.Achievements;

[CreateAssetMenu(fileName = "New Parent Achievement Definition", menuName = "CodeRebirthLib/Definitions/Achievements/Parent Definition")]
public class CRParentAchievement : CRAchievementBaseDefinition, IProgress
{
    [field: SerializeField]
    public List<string> ChildrenAchievementNames { get; private set; } = new();

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
        return Mod.AchievementRegistry()
            .Where(it => ChildrenAchievementNames.Contains(it.AchievementName))
            .Count(it => it.Completed);
    }

    public override bool IsActive()
    {
        return Mod.AchievementRegistry().Count(it => ChildrenAchievementNames.Contains(it.AchievementName)) == ChildrenAchievementNames.Count;
    }

    public float MaxProgress => ChildrenAchievementNames.Count;
    public float CurrentProgress => CountCompleted();
}