using System;
using System.Text;
using CodeRebirthLib.Extensions;
using CodeRebirthLib.Util;

namespace CodeRebirthLib.ContentManagement.Unlockables.Progressive;
public class ProgressiveUnlockData
{
    public CRUnlockableDefinition Definition { get; }
    public bool IsUnlocked { get; private set; }

    public const string LOCKED_NAME = "???";
    public string OriginalName { get; private set; }
    
    private UnlockableItem _unlockable => Definition.UnlockableItemDef.unlockable;
    
    private string _saveID => _unlockable.ToString(); // todo: something better than this
    internal uint networkID => BitConverter.ToUInt32(Encoding.UTF8.GetBytes(_saveID), 0);
    
    public ProgressiveUnlockData(CRUnlockableDefinition definition)
    {
        Definition = definition;
        OriginalName = _unlockable.unlockableName;
        ProgressiveUnlockableHandler.AllProgressiveUnlockables.Add(this);
    }

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
        IsUnlocked = ES3.Load(_saveID, false, settings);
        // todo: add override?
        UpdateName();
        CodeRebirthLibPlugin.ExtendedLogging($"IsUnlocked: {IsUnlocked}, Loaded unlockable: {_unlockable.unlockableName} with saveID: {_saveID}");
    }

    public void Save(ES3Settings settings)
    {
        CodeRebirthLibPlugin.ExtendedLogging($"Saving unlockable: {_unlockable.unlockableName} with original name: {OriginalName} that is unlocked: {IsUnlocked} with saveID: {_saveID}");
        ES3.Save(_saveID, IsUnlocked, settings);
    }

    internal void SetFromServer(bool isUnlocked)
    {
        CodeRebirthLibPlugin.ExtendedLogging($"{OriginalName} is being set from the server; unlocked = {isUnlocked}");
        IsUnlocked = isUnlocked;
        UpdateName();
    }

    private void UpdateName()
    {
        _unlockable.unlockableName = IsUnlocked ? OriginalName : LOCKED_NAME;
    }
}