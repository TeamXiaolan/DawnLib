using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CodeRebirthLib.ContentManagement.Achievements;

[CreateAssetMenu(fileName = "New Parent Achievement Definition", menuName = "CodeRebirthLib/Definitions/Achievements/Parent Definition")]
public class CRParentAchievement : CRAchievementBaseDefinition
{
    [SerializeField]
    private List<string> _childrenAchievementNames;

    public override void Register(CRMod mod)
    {
        base.Register(mod);
        CRMod.OnAchievementUnlocked += definition =>
        {
            if(definition.Mod != mod) return;
            if (CheckAllModsCompleted())
            {
                TryCompleteAchievement();
            }
        };
    }

    bool CheckAllModsCompleted()
    {
        return Mod.AchievementRegistry()
            .Where(it => _childrenAchievementNames.Contains(it.AchievementName))
            .All(it => it.Completed);
    }

    public override bool IsActive()
    {
        return Mod.AchievementRegistry().Count(it => _childrenAchievementNames.Contains(it.AchievementName)) == _childrenAchievementNames.Count;
    }
}