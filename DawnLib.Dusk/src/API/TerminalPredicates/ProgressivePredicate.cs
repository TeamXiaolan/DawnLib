using System;
using System.Collections.Generic;
using System.Text;
using Dawn;
using Dawn.Internal;
using Dawn.Utils;
using UnityEngine;

namespace Dusk;

[Serializable]
public class ProgressiveStates(bool isUnlocked, bool isHidden)
{
    public bool IsUnlocked { get; internal set; } = isUnlocked;
    public bool IsHidden { get; internal set; } = isHidden;
}

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

    public ProgressiveStates ProgressiveStates { get; private set; }
    private NamespacedKey _namespacedKey;
    internal uint NetworkID => BitConverter.ToUInt32(Encoding.UTF8.GetBytes(_namespacedKey.ToString()), 0);

    public override void Register(NamespacedKey namespacedKey)
    {
        if (!_failNode)
        {
            _failNode = CreateDefaultProgressiveDenyNode();
        }

        _namespacedKey = namespacedKey;
        ProgressiveStates = new ProgressiveStates(!_isLocked, _isHidden);
        AllProgressiveItems.Add(this);
        Debuggers.Progressive?.Log($"Registering unlockable: {_namespacedKey}.");
    }

    public override TerminalPurchaseResult CanPurchase()
    {
        if (ProgressiveStates.IsHidden && ProgressiveStates.IsUnlocked)
        {
            return TerminalPurchaseResult.Hidden()
                .SetFailure(false);
        }
        else if (ProgressiveStates.IsHidden)
        {
            return TerminalPurchaseResult.Hidden()
                .SetFailure(true)
                .SetFailNode(_failNode);
        }
        else if (!ProgressiveStates.IsUnlocked && _isLocked)
        {
            return TerminalPurchaseResult.Fail(_failNode)
                .SetOverrideName(_lockedName);
        }
        return TerminalPurchaseResult.Success();
    }

    public void Unlock(HUDDisplayTip? displayTip = null)
    {
        if (ProgressiveStates.IsUnlocked)
            return;

        ProgressiveStates.IsUnlocked = true;

        if (displayTip != null)
        {
            HUDManager.Instance.DisplayTip(displayTip);
        }
    }

    public void Unhide()
    {
        ProgressiveStates.IsHidden = false;
    }

    public void Load(IDataContainer dataContainer)
    {
        if (dataContainer.Has<bool>(_namespacedKey))
        {
            ProgressiveStates.IsUnlocked = dataContainer.GetOrSetDefault(_namespacedKey, !_isLocked);
            Debuggers.Progressive?.Log($"Used older save data for unlockable: {_namespacedKey} for unlocked: {ProgressiveStates.IsUnlocked} with saveID: {_namespacedKey}");
            return;
        }
        ProgressiveStates = dataContainer.GetOrSetDefault(_namespacedKey, new ProgressiveStates(!_isLocked, _isHidden));
        Debuggers.Progressive?.Log($"IsUnlocked: {ProgressiveStates.IsUnlocked}, IsHidden: {ProgressiveStates.IsHidden}, Loaded unlockable: {_namespacedKey} with saveID: {_namespacedKey}");
    }

    public void Save(IDataContainer dataContainer)
    {
        Debuggers.Progressive?.Log($"Saving unlockable: {_namespacedKey} that is unlocked: {ProgressiveStates.IsUnlocked} with saveID: {_namespacedKey}");
        dataContainer.Set(_namespacedKey, ProgressiveStates);
    }

    internal static List<ProgressivePredicate> AllProgressiveItems = new();

    internal static void LoadAll(IDataContainer dataContainer)
    {
        Debuggers.Progressive?.Log("Loading all unlockables");
        foreach (ProgressivePredicate unlockData in AllProgressiveItems)
        {
            unlockData.Load(dataContainer);
        }
    }

    internal static void SaveAll(IDataContainer dataContainer)
    {
        Debuggers.Progressive?.Log("Saving all unlockables");
        foreach (ProgressivePredicate unlockData in AllProgressiveItems)
        {
            unlockData.Save(dataContainer);
        }
    }

    internal void SetFromServer(bool isUnlocked, bool isHidden)
    {
        Debuggers.Progressive?.Log($"{_namespacedKey} is being set from the server; unlocked = {isUnlocked}");
        ProgressiveStates.IsUnlocked = isUnlocked;
        ProgressiveStates.IsHidden = isHidden;
    }

    private static TerminalNode CreateDefaultProgressiveDenyNode()
    {
        TerminalNode node = CreateInstance<TerminalNode>();
        node.displayText = "Ship Upgrade or Decor is not unlocked";
        return node;
    }
}