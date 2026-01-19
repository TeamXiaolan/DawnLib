using System;
using System.Collections.Generic;
using UnityEngine;
using Dawn.Internal;
using Dawn.Utils;

namespace Dawn;
public class TerminalKeywordBuilder
{
    internal static List<TerminalKeyword> _terminalKeywords = new List<TerminalKeyword>();
    private TerminalKeyword _keyword;

    internal TerminalKeywordBuilder(string name, string word, ITerminalKeyword.DawnKeywordType keywordPriority)
    {
        bool terminalExists = TerminalRefs.Instance != null;
        if ((terminalExists && TerminalRefs.Instance!.TryGetKeyword(word, out TerminalKeyword existingKeywordWithSameWord)) || (!terminalExists && NoTerminalTryGetKeyword(word, out existingKeywordWithSameWord)))
        {
            ITerminalKeyword.DawnKeywordType existingPriority = existingKeywordWithSameWord.GetKeywordPriority();
            if (existingPriority <= keywordPriority)
            {
                DawnPlugin.Logger.LogWarning($"'{word}' already has an existing TerminalKeyword with a higher priority [ {existingPriority} ]");
                //below still creates a new keyword with just a unique (and unusuable) replacement word.
                //throwing an exception here breaks the terminal and returning a null keyword will just break the terminal later
                _keyword = ScriptableObject.CreateInstance<TerminalKeyword>();
                _keyword.name = name;
                _keyword.SetKeywordPriority(keywordPriority);
                OverrideWord(word + _keyword.GetHashCode());
                DawnPlugin.Logger.LogWarning($"TerminalKeyword word set to {_keyword.word}");
                return;
            }
            else
            {
                _keyword = existingKeywordWithSameWord;
                DawnPlugin.Logger.LogWarning($"Replacing keyword [{_keyword.word}] with priority [ {existingPriority} ] due to new keyword with the same word at a higher priority [ {keywordPriority} ]");
                _keyword.name = name;
                _keyword.SetKeywordPriority(keywordPriority);
                return;
            }
        }

        _keyword = ScriptableObject.CreateInstance<TerminalKeyword>();
        _keyword.name = name;
        _keyword.SetKeywordPriority(keywordPriority);
        OverrideWord(word);
    }

    public TerminalKeywordBuilder OverrideWord(string word)
    {
        _keyword.word = word;
        return this;
    }

    [Obsolete("Use OverrideWord()")]
    public TerminalKeywordBuilder SetWord(string word)
    {
        return OverrideWord(word);
    }

    public TerminalKeywordBuilder SetIsVerb(bool isVerb)
    {
        _keyword.isVerb = isVerb;
        return this;
    }

    public TerminalKeywordBuilder AddCompatibleNoun(CompatibleNoun compatibleNoun)
    {
        List<CompatibleNoun> compatibleNouns = [.. _keyword.compatibleNouns];
        compatibleNouns.Add(compatibleNoun);
        _keyword.compatibleNouns = [.. compatibleNouns];
        return this;
    }

    public TerminalKeywordBuilder SetSpecialKeywordResult(TerminalNode specialKeywordResult)
    {
        _keyword.specialKeywordResult = specialKeywordResult;
        return this;
    }

    public TerminalKeywordBuilder SetAccessTerminalObjects(bool accessTerminalObjects)
    {
        _keyword.accessTerminalObjects = accessTerminalObjects;
        return this;
    }

    public TerminalKeywordBuilder SetDefaultVerb(TerminalKeyword defaultVerb)
    {
        _keyword.defaultVerb = defaultVerb;
        return this;
    }

    public TerminalKeywordBuilder SetNodeFunction(Func<string> _function)
    {
        _keyword.specialKeywordResult.SetNodeFunction(_function);
        return this;
    }

    //only need to use this to set the value to true, default is false
    public TerminalKeywordBuilder SetAcceptInput(bool value)
    {
        _keyword.SetKeywordAcceptInput(value);
        return this;
    }

    public TerminalKeyword Build()
    {
        return _keyword;
    }

    public static bool NoTerminalTryGetKeyword(string keyWord, out TerminalKeyword terminalKeyword)
    {
        foreach (TerminalKeyword keyword in _terminalKeywords)
        {
            if (keyWord.CompareStringsInvariant(keyword.word))
            {
                //Loggers.LogDebug($"Keyword: [{keyWord}] found!");
                terminalKeyword = keyword;
                return true;
            }
        }

        terminalKeyword = null!;
        return false;
    }
}
