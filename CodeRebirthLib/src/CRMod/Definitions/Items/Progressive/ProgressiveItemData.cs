using System;
using System.Text;
using CodeRebirthLib.Internal;
using CodeRebirthLib.Utils;

namespace CodeRebirthLib.CRMod;
public class ProgressiveItemData
{
    public const string LOCKED_NAME = "???";

    public ProgressiveItemData(CRMItemDefinition definition)
    {
        Definition = definition;
        OriginalName = Item.itemName;
        ProgressiveItemHandler.AllProgressiveItems.Add(this);
    }

    public CRMItemDefinition Definition { get; }
    public bool IsUnlocked { get; private set; }
    public string OriginalName { get; }

    private Item Item => Definition.Item;

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
        Debuggers.Progressive?.Log($"IsUnlocked: {IsUnlocked}, Loaded unlockable: {Item.itemName} with saveID: {SaveID}");
    }

    public void Save(ES3Settings settings)
    {
        Debuggers.Progressive?.Log($"Saving unlockable: {Item.itemName} with original name: {OriginalName} that is unlocked: {IsUnlocked} with saveID: {SaveID}");
        ES3.Save(SaveID, IsUnlocked, settings);
    }

    internal void SetFromServer(bool isUnlocked)
    {
        Debuggers.Progressive?.Log($"{OriginalName} is being set from the server; unlocked = {isUnlocked}");
        IsUnlocked = isUnlocked;
        UpdateName();
    }

    private void UpdateName()
    {
        Item.itemName = IsUnlocked ? OriginalName : LOCKED_NAME;
    }
}