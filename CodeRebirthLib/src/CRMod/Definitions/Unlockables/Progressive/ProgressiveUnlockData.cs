using System;
using System.Text;
using CodeRebirthLib.Utils;

namespace CodeRebirthLib.CRMod;
public class ProgressiveUnlockData
{
    public const string LOCKED_NAME = "???";

    public ProgressiveUnlockData(CRUnlockableDefinition definition)
    {
        Definition = definition;
        OriginalName = Unlockable.unlockableName;
        ProgressiveUnlockableHandler.AllProgressiveUnlockables.Add(this);
    }

    public CRUnlockableDefinition Definition { get; }
    public bool IsUnlocked { get; private set; }
    public string OriginalName { get; }

    private UnlockableItem Unlockable => Definition.UnlockableItem;

    private string SaveID => OriginalName;
    internal uint NetworkID => BitConverter.ToUInt32(Encoding.UTF8.GetBytes(SaveID), 0);

    public void Unlock(HUDDisplayTip? displayTip = null)
    {
        if (IsUnlocked)
            return;

        IsUnlocked = true;
        UpdateName();

        if (displayTip != null)
        {
            HUDManager.Instance.DisplayTip(displayTip);
        }
    }

    public void Load(ES3Settings settings)
    {
        IsUnlocked = ES3.Load(SaveID, false, settings);
        UpdateName();
        Debuggers.ReplaceThis?.Log($"IsUnlocked: {IsUnlocked}, Loaded unlockable: {Unlockable.unlockableName} with saveID: {SaveID}");
    }

    public void Save(ES3Settings settings)
    {
        Debuggers.ReplaceThis?.Log($"Saving unlockable: {Unlockable.unlockableName} with original name: {OriginalName} that is unlocked: {IsUnlocked} with saveID: {SaveID}");
        ES3.Save(SaveID, IsUnlocked, settings);
    }

    internal void SetFromServer(bool isUnlocked)
    {
        Debuggers.ReplaceThis?.Log($"{OriginalName} is being set from the server; unlocked = {isUnlocked}");
        IsUnlocked = isUnlocked;
        UpdateName();
    }

    private void UpdateName()
    {
        Unlockable.unlockableName = IsUnlocked ? OriginalName : LOCKED_NAME;
    }
}