using System.Collections.Generic;
using System.Linq;
using CodeRebirthLib.Internal;
using TMPro;
using UnityEngine;

namespace CodeRebirthLib.CRMod;

/* TODOs
 * Button to reset specific achievements
 * probably more im forgetting
*/
public abstract class CRMAchievementDefinition : CRMContentDefinition<AchievementData, CRMAchievementDefinition>, INamespaced<CRMAchievementDefinition>
{
    public const string REGISTRY_ID = "achievements";

    [field: SerializeField]
    public Sprite? AchievementIcon { get; private set; }

    [field: Space(10)]
    [field: SerializeField]
    public string AchievementName { get; private set; }
    [field: SerializeField]
    public string AchievementDescription { get; private set; }

    [field: SerializeField]
    public TMP_ColorGradient? FinishedAchievementNameColorGradientPreset { get; private set; }
    [field: SerializeField]
    public TMP_ColorGradient? FinishedAchievementDescColorGradientPreset { get; private set; }
    [field: SerializeField]
    public Sprite? FinishedAchievementBackgroundIcon { get; private set; }

    [field: Space(10)]
    [field: SerializeField]
    public string AchievementRequirement { get; private set; }

    [field: SerializeField]
    public bool IsHidden { get; private set; }
    [field: SerializeField]
    public float PopupTime { get; private set; } = 5f;

    public bool Completed { get; protected set; } = false;

    protected override string EntityNameReference => AchievementName;

    NamespacedKey<CRMAchievementDefinition> INamespaced<CRMAchievementDefinition>.TypedKey => TypedKey;

    public virtual void LoadAchievementState(ES3Settings globalSettings)
    {
        Completed = ES3.Load(Mod.Plugin.GUID + "." + AchievementName, false, globalSettings);
        Debuggers.Achievements?.Log($"Loaded Achievement: {AchievementName} with value: {Completed}");
    }

    public virtual void SaveAchievementState(ES3Settings globalSettings)
    {
        ES3.Save(Mod.Plugin.GUID + "." + AchievementName, Completed, globalSettings);
        Debuggers.Achievements?.Log($"Saving Achievement: {AchievementName} with value: {Completed}");
    }

    protected bool TryCompleteAchievement()
    {
        if (Completed)
        {
            return false;
        }

        Completed = true;
        AchievementUIGetCanvas.Instance?.QueuePopup(this);
        return Completed;
    }

    public virtual void ResetProgress() // TODO do i need to reload all achievements after this so that parent achievement actually updates for this same with the UI?
    {
        Completed = false;
    }

    public override void Register(CRMod mod, AchievementData data)
    {
        base.Register(mod);
        CRModContent.Achievements.Register(this);
    }

    public override List<AchievementData> GetEntities(CRMod mod)
    {
        return mod.Content.assetBundles.SelectMany(it => it.achievements).ToList();
    }

    public virtual bool IsActive() { return true; }
}