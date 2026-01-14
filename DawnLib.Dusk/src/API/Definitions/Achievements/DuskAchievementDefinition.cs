using Dawn;
using Dawn.Internal;
using TMPro;
using UnityEngine;

namespace Dusk;

public abstract class DuskAchievementDefinition : DuskContentDefinition, INamespaced<DuskAchievementDefinition>
{
    [field: SerializeField]
    private NamespacedKey<DuskAchievementDefinition> _typedKey;

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
    [field: SerializeField]
    public AudioClip? FinishAchievementAudioClip { get; private set; }

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

    public NamespacedKey<DuskAchievementDefinition> TypedKey => _typedKey;
    public override NamespacedKey Key { get => TypedKey; protected set => _typedKey = value.AsTyped<DuskAchievementDefinition>(); }

    protected virtual AchievementSaveData GetSaveData()
    {
        return new AchievementSaveData(Completed);
    }

    protected virtual void LoadSaveData(AchievementSaveData saveData)
    {
        Completed = saveData.Completed;
    }

    public void LoadAchievementState(IDataContainer dataContainer)
    {
        LoadSaveData(dataContainer.GetOrSetDefault(Key, GetSaveData()));
        Debuggers.Achievements?.Log($"Loaded Achievement: {AchievementName} with value: {Completed}");
    }

    public void SaveAchievementState(IDataContainer dataContainer)
    {
        dataContainer.Set(Key, GetSaveData());
        Debuggers.Achievements?.Log($"Saving Achievement: {AchievementName} with value: {Completed}");
    }

    protected bool TryCompleteAchievement()
    {
        if (Completed)
        {
            return false;
        }

        Completed = true;
        DuskAchievementHandler.OnAchievementUnlocked.Invoke(this);
        return Completed;
    }

    internal bool TryCompleteFromServer()
    {
        return TryCompleteAchievement();
    }

    public virtual void ResetProgress()
    {
        Completed = false;
        DuskAchievementHandler.SaveAll();
        DuskAchievementHandler.LoadAll();
        foreach (AchievementModUIElement modUIElement in AchievementModUIElement.achievementModUIElements)
        {
            foreach (AchievementUIElement achievementUIElement in modUIElement.achievementsContainerList)
            {
                DuskAchievementHandler.UpdateUIElement(achievementUIElement, achievementUIElement.achievementDefinition);
            }
        }
    }

    public override void Register(DuskMod mod)
    {
        base.Register(mod);
        DuskModContent.Achievements.Register(this);
    }

    public virtual bool IsActive() { return true; }

    protected override string EntityNameReference => AchievementName;
}