using System;
using System.Collections.Generic;
using UnityEngine;

namespace Dawn;
public class TerminalNodeBuilder
{
    internal static List<TerminalNode> _terminalNodes = new();
    private TerminalNode _node;

    internal TerminalNodeBuilder(string name)
    {
        _node = ScriptableObject.CreateInstance<TerminalNode>();
        _node.name = name;

        _terminalNodes.Add(_node);
    }

    public TerminalNodeBuilder SetBuyItemIndex(int index)
    {
        _node.buyItemIndex = index;
        return this;
    }

    public TerminalNodeBuilder SetBuyVehicleIndex(int index)
    {
        _node.buyVehicleIndex = index;
        return this;
    }

    public TerminalNodeBuilder SetShipUnlockableIndex(int index)
    {
        _node.shipUnlockableID = index;
        return this;
    }

    public TerminalNodeBuilder SetDisplayText(string text)
    {
        _node.displayText = text;
        return this;
    }

    public TerminalNodeBuilder SetClearPreviousText(bool clearPreviousText)
    {
        _node.clearPreviousText = clearPreviousText;
        return this;
    }

    public TerminalNodeBuilder SetMaxCharactersToType(int maxCharacters)
    {
        _node.maxCharactersToType = maxCharacters;
        return this;
    }

    public TerminalNodeBuilder SetItemCost(int cost)
    {
        _node.itemCost = cost;
        return this;
    }

    public TerminalNodeBuilder SetCreatureName(string creatureName)
    {
        _node.creatureName = creatureName;
        return this;
    }

    public TerminalNodeBuilder SetOverrideOptions(bool overrideOptions)
    {
        _node.overrideOptions = overrideOptions;
        return this;
    }

    public TerminalNodeBuilder SetTerminalOptions(CompatibleNoun[] terminalOptions)
    {
        _node.terminalOptions = terminalOptions;
        return this;
    }

    public TerminalNodeBuilder SetIsConfirmationNode(bool isConfirmationNode)
    {
        _node.isConfirmationNode = isConfirmationNode;
        return this;
    }

    public TerminalNodeBuilder SetBuyUnlockable(bool buyUnlockable)
    {
        _node.buyUnlockable = buyUnlockable;
        return this;
    }

    public TerminalNodeBuilder SetPlaySyncedClip(int clipIndex)
    {
        _node.playSyncedClip = clipIndex;
        return this;
    }

    public TerminalNode Build()
    {
        return _node;
    }
}