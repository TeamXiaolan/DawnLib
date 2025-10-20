using System;
using System.Collections.Generic;
using System.Text;
using Dawn;
using Dawn.Internal;
using Dawn.Utils;
using UnityEngine;

namespace Dusk;
[CreateAssetMenu(menuName = $"{DuskModConstants.TerminalPredicates}/Progressive Unlockable", fileName = "New Progressive Predicate", order = DuskModConstants.PredicateOrder)]
public class ProgressivePredicate : DuskTerminalPredicate
{
    [SerializeField]
    bool _isHidden;

    [SerializeField]
    bool _isLocked;

    [SerializeField]
    string _lockedName = "???";

    [SerializeField]
    private TerminalNode _failNode;

    public bool IsUnlocked { get; private set; }

    private NamespacedKey _namespacedKey;
    internal uint NetworkID => BitConverter.ToUInt32(Encoding.UTF8.GetBytes(_namespacedKey.ToString()), 0);

    public override void Register(NamespacedKey namespacedKey)
    {
        if (!_failNode)
        {
            _failNode = CreateDefaultProgressiveDenyNode();
        }
        _namespacedKey = namespacedKey;
    }

    public override TerminalPurchaseResult CanPurchase()
    {
        if (IsUnlocked)
        {
            return TerminalPurchaseResult.Success();
        }

        if (_isLocked && _failNode)
        {
            return TerminalPurchaseResult.Fail(_failNode)
                .SetOverrideName(_lockedName);
        }
        else
        {
            return TerminalPurchaseResult.Hidden();
        }
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

    public void Load(IDataContainer dataContainer)
    {
        IsUnlocked = dataContainer.GetOrSetDefault(_namespacedKey, false);
        Debuggers.Progressive?.Log($"IsUnlocked: {IsUnlocked}, Loaded unlockable: {_namespacedKey} with saveID: {_namespacedKey}");
    }

    public void Save(IDataContainer dataContainer)
    {
        Debuggers.Progressive?.Log($"Saving unlockable: {_namespacedKey} that is unlocked: {IsUnlocked} with saveID: {_namespacedKey}");
        dataContainer.Set(_namespacedKey, IsUnlocked);
    }

    internal static List<ProgressivePredicate> AllProgressiveItems = new();

    internal static void LoadAll(IDataContainer dataContainer)
    {
        foreach (ProgressivePredicate unlockData in AllProgressiveItems)
        {
            unlockData.Load(dataContainer);
        }
    }

    internal static void SaveAll(IDataContainer dataContainer)
    {
        foreach (ProgressivePredicate unlockData in AllProgressiveItems)
        {
            unlockData.Save(dataContainer);
        }
    }

    internal void SetFromServer(bool isUnlocked)
    {
        Debuggers.Progressive?.Log($"{_namespacedKey} is being set from the server; unlocked = {isUnlocked}");
        IsUnlocked = isUnlocked;
    }

    private static TerminalNode CreateDefaultProgressiveDenyNode()
    {
        TerminalNode node = CreateInstance<TerminalNode>();
        node.displayText = "Ship Upgrade or Decor is not unlocked";
        return node;
    }
}