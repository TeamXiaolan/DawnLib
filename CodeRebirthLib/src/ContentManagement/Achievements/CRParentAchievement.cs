using System.Collections.Generic;
using System.Linq;
using CodeRebirthLib.Data;
using UnityEngine;

namespace CodeRebirthLib.ContentManagement.Achievements;

[CreateAssetMenu(fileName = "New Parent Achievement Definition", menuName = "CodeRebirthLib/Definitions/Achievements/Parent Definition")]
public class CRParentAchievement : CRAchievementBaseDefinition, IProgress
{
    [SerializeField]
    private List<string> _childrenAchievementNames;

    public override void Register(CRMod mod)
    {
        base.Register(mod);
        CRMod.OnAchievementUnlocked += definition =>
        {
            if(definition.Mod != mod) return;
            if (CountCompleted() >= _childrenAchievementNames.Count)
            {
                TryCompleteAchievement();
            }
        };
    }

    int CountCompleted()
    {
        return Mod.AchievementRegistry()
            .Where(it => _childrenAchievementNames.Contains(it.AchievementName))
            .Count(it => it.Completed);
    }

    public override bool IsActive()
    {
        return Mod.AchievementRegistry().Count(it => _childrenAchievementNames.Contains(it.AchievementName)) == _childrenAchievementNames.Count;
    }
    public float MaxProgress => _childrenAchievementNames.Count;
    public float CurrentProgress => CountCompleted();
}