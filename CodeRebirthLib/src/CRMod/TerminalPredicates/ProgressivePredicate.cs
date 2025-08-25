using System;
using System.Collections.Generic;
using System.Text;
using CodeRebirthLib.Internal;
using CodeRebirthLib.Utils;
using LethalLib.Modules;
using UnityEngine;

namespace CodeRebirthLib.CRMod;
[CreateAssetMenu(menuName = "CodeRebirthLib/Terminal Predicates/Progressive Unlockable", fileName = "New Progressive Predicate", order = -40)]
public class ProgressivePredicate : CRMTerminalPredicate
{
    [SerializeField]
    string _lockedName = "???";

    [SerializeField]
    private TerminalNode _failNode;
    
    public bool IsUnlocked { get; private set; }

    private string _saveID;
    internal uint NetworkID => BitConverter.ToUInt32(Encoding.UTF8.GetBytes(_saveID), 0);

    public override void Register(string id)
    {
        if (!_failNode)
        {
            _failNode = CreateDefaultProgressiveDenyNode();
        }
        _saveID = id;
    }
    
    public override TerminalPurchaseResult CanPurchase()
    {
        if (IsUnlocked) return TerminalPurchaseResult.Success();
        return TerminalPurchaseResult.Fail(_failNode, _lockedName);
    }
    
    public void Unlock(HUDDisplayTip? displayTip = null)
    {
        if (IsUnlocked)
            return;

        IsUnlocked = true;

        if (displayTip != null)
        {
            HUDManager.Instance.DisplayTip(displayTip);
        }
    }
    
    public void Load(ES3Settings settings)
    {
        IsUnlocked = ES3.Load(_saveID, false, settings);
        Debuggers.Progressive?.Log($"IsUnlocked: {IsUnlocked}, Loaded unlockable: {_saveID} with saveID: {_saveID}");
    }

    public void Save(ES3Settings settings)
    {
        Debuggers.Progressive?.Log($"Saving unlockable: {_saveID} that is unlocked: {IsUnlocked} with saveID: {_saveID}");
        ES3.Save(_saveID, IsUnlocked, settings);
    }
    
    internal static List<ProgressivePredicate> AllProgressiveItems = new();

    internal static void LoadAll(ES3Settings settings)
    {
        foreach (ProgressivePredicate unlockData in AllProgressiveItems)
        {
            unlockData.Load(settings);
        }
    }

    internal static void SaveAll(ES3Settings settings)
    {
        foreach (ProgressivePredicate unlockData in AllProgressiveItems)
        {
            unlockData.Save(settings);
        }
    }
    
    internal void SetFromServer(bool isUnlocked)
    {
        Debuggers.Progressive?.Log($"{_saveID} is being set from the server; unlocked = {isUnlocked}");
        IsUnlocked = isUnlocked;
    }
    
    private static TerminalNode CreateDefaultProgressiveDenyNode()
    {
        TerminalNode node = CreateInstance<TerminalNode>();
        node.displayText = "Ship Upgrade or Decor is not unlocked";
        return node;
    }
}