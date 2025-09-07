using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Dawn;
public class TerminalKeywordBuilder
{
    private TerminalKeyword _keyword;

    internal TerminalKeywordBuilder(string name)
    {
        _keyword = ScriptableObject.CreateInstance<TerminalKeyword>();
        _keyword.name = name;
    }

    public TerminalKeywordBuilder SetWord(string word)
    {
        _keyword.word = word;
        return this;
    }

    public TerminalKeywordBuilder SetIsVerb(bool isVerb)
    {
        _keyword.isVerb = isVerb;
        return this;
    }

    public TerminalKeywordBuilder AddCompatibleNoun(CompatibleNoun compatibleNoun)
    {
        List<CompatibleNoun> compatibleNouns = _keyword.compatibleNouns.ToList();
        compatibleNouns.Add(compatibleNoun);
        _keyword.compatibleNouns = compatibleNouns.ToArray();
        return this;
    }

    public TerminalKeywordBuilder SetSpecialKeywordResult(TerminalNode specialKeywordResult)
    {
        _keyword.specialKeywordResult = specialKeywordResult;
        return this;
    }

    public TerminalKeywordBuilder AccessTerminalObjects(bool accessTerminalObjects)
    {
        _keyword.accessTerminalObjects = accessTerminalObjects;
        return this;
    }

    public TerminalKeywordBuilder SetDefaultVerb(TerminalKeyword defaultVerb)
    {
        _keyword.defaultVerb = defaultVerb;
        return this;
    }

    public TerminalKeyword Build()
    {
        return _keyword;
    }
}