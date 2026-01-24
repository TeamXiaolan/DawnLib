using System;
using System.Collections.Generic;
using UnityEngine;
using Dawn.Internal;
using Dawn.Utils;

namespace Dawn;
public class TerminalKeywordBuilder
{
    private static List<TerminalKeyword> _keywordsAdded = [];
    internal static List<TerminalKeyword> AllTerminalKeywords
    {
        get
        {
            //get initial existing keywords
            if(_keywordsAdded.Count == 0)
            {
                _keywordsAdded = [.. Resources.FindObjectsOfTypeAll<TerminalKeyword>()];
            }
            
            return _keywordsAdded;
        }
        private set
        {
            _keywordsAdded = value;
        }
    }
    internal static List<TerminalKeyword> WordsThatAcceptInput { get; private set; } = [];
    private TerminalKeyword _keyword;

    private static bool WordAlreadyExists(string word, out TerminalKeyword existingKeyword)
    {
        if(TerminalRefs.Instance == null)
        {
            existingKeyword = null!;
            foreach (TerminalKeyword keyword in AllTerminalKeywords)
            {
                if (word.CompareStringsInvariant(keyword.word))
                {
                    //Loggers.LogDebug($"Keyword: [{keyWord}] found!");
                    existingKeyword = keyword;
                }
            }

            return existingKeyword != null;
        }
        else
        {
            TerminalRefs.Instance.TryGetKeyword(word, out existingKeyword);
            return existingKeyword != null;
        }
    }

    internal TerminalKeywordBuilder(string name, string word, ITerminalKeyword.DawnKeywordType keywordPriority)
    {
        if (WordAlreadyExists(word, out TerminalKeyword existingKeywordWithSameWord))
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
                AllTerminalKeywords.Add(_keyword);
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
        AllTerminalKeywords.Add(_keyword);
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

    public TerminalKeywordBuilder SetAcceptInput(bool value)
    {
        if (value)
        {
            WordsThatAcceptInput.Add(_keyword);
        }
        else
        {
            WordsThatAcceptInput.Remove(_keyword);
        }

        _keyword.SetKeywordAcceptInput(value);
        return this;
    }

    public TerminalKeyword Build()
    {
        return _keyword;
    }
}
