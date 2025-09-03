using CodeRebirthLib.Internal;
using TMPro;
using UnityEngine;

namespace CodeRebirthLib.CRMod;

public abstract class CRMAchievementDefinition : CRMContentDefinition, INamespaced<CRMAchievementDefinition>
{
    [field: SerializeField]
    private NamespacedKey<CRMAchievementDefinition> _typedKey;

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
    public bool CanBeUnhidden { get; private set; }

    [field: SerializeField]
    public float PopupTime { get; private set; } = 5f;

    public bool Completed { get; protected set; } = false;

    public NamespacedKey<CRMAchievementDefinition> TypedKey => _typedKey;
    public override NamespacedKey Key { get => TypedKey; protected set => throw new System.NotImplementedException(); } // TODO

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

    public virtual void ResetProgress()
    {
        Completed = false;

        CRAchievementHandler.SaveAll();
        CRAchievementHandler.LoadAll();
        foreach (AchievementModUIElement modUIElement in AchievementModUIElement.achievementModUIElements)
        {
            foreach (AchievementUIElement achievementUIElement in modUIElement.achievementsContainerList)
            {
                CRAchievementHandler.UpdateUIElement(achievementUIElement, achievementUIElement.achievementDefinition);
            }
        }
    }

    public override void Register(CRMod mod)
    {
        base.Register(mod);
        CRModContent.Achievements.Register(this);
    }

    public virtual bool IsActive() { return true; }

    protected override string EntityNameReference => AchievementName;
}