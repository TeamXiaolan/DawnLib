using UnityEngine;

namespace Dawn;
public class TerminalNodeBuilder
{
    private TerminalNode _node;

    internal TerminalNodeBuilder(string name)
    {
        _node = ScriptableObject.CreateInstance<TerminalNode>();
        _node.name = name;
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

    public TerminalNode Build()
    {
        return _node;
    }
}