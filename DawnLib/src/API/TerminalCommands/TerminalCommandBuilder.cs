using System;
using System.Collections.Generic;
using Dawn.Internal;
using UnityEngine;
using UnityEngine.Events;

namespace Dawn;

public class TerminalCommandBuilder
{
    private string _commandName = string.Empty;
    private TerminalNode _resultNode = null!;
    //list allows for multiple keywords to be added for the same result/query node
    //Also is used to track everything that is created for a command (nodes, keywords, and compatible nouns) for deletion at terminaldisable
    private List<TerminalKeyword> _validKeywords = [];

    //Required for Query-style commands (confirm/deny)
    private TerminalNode? _queryNode; //for nodes that have compatible nouns like "confirm or deny" which query the user 2 choices
    private TerminalNode? _cancelNode; //same as query node, this is the result of denying from the query node
    private TerminalKeyword? _continueWord; //word used to continue after a query
    private TerminalKeyword? _cancelWord;


    public TerminalCommandBuilder(string name)
    {
        _commandName = name;
        TerminalPatches.OnTerminalDisable += Destroy;
    }

    internal void SetCustomDestroyEvent(UnityEvent destroyEvent)
    {
        TerminalPatches.OnTerminalDisable -= Destroy;
        destroyEvent.AddListener(Destroy);
    }

    //Destroy created commands before next TerminalAwake
    //While some commands should always exist, we should not assume all commands are built this way.
    //Also, deleting them on TerminalDisable allows for configuration items to determine if a command should be enabled/disabled on lobby reload
    private void Destroy()
    {
        if (_validKeywords.Count == 0)
            return;

        List<TerminalKeyword> terminalWords = [.. TerminalRefs.Instance.terminalNodes.allKeywords];

        for (int i = _validKeywords.Count - 1; i >= 0; i--)
        {
            if (_validKeywords[i] == null)
                continue;

            terminalWords.Remove(_validKeywords[i]);

            Debuggers.Terminal?.Log($"Deleting command (keyword, compatible nouns, and related terminal nodes): {_validKeywords[i].name}");

            //destroy results and nouns first
            if (_validKeywords[i].compatibleNouns != null && _validKeywords[i].compatibleNouns.Length > 0)
            {
                for (int c = _validKeywords[i].compatibleNouns.Length - 1; c >= 0; c--)
                {
                    if (_validKeywords[i].compatibleNouns[c].result != null && _validKeywords[i].compatibleNouns[c].result != _resultNode)
                    {
                        UnityEngine.Object.Destroy(_validKeywords[i].compatibleNouns[c].result);
                    }

                    if (_validKeywords[i].compatibleNouns[c].noun != null)
                    {
                        UnityEngine.Object.Destroy(_validKeywords[i].compatibleNouns[c].noun);
                    }
                }
            }

            //then special result node
            if (_validKeywords[i].specialKeywordResult != null && _validKeywords[i].specialKeywordResult != _resultNode)
            {
                UnityEngine.Object.Destroy(_validKeywords[i].specialKeywordResult);
            }

            //then keyword itself
            UnityEngine.Object.Destroy(_validKeywords[i]);
        }

        TerminalRefs.Instance.terminalNodes.allKeywords = [.. terminalWords];
    }

    public TerminalCommandBuilder AddKeyword(TerminalKeyword word)
    {
        if (!_validKeywords.Contains(word))
        {
            _validKeywords.Add(word);
        }

        return this;
    }

    public TerminalCommandBuilder AddKeyword(List<TerminalKeyword> words)
    {
        foreach (TerminalKeyword keyword in words)
        {
            AddKeyword(keyword);
        }

        return this;
    }

    public TerminalCommandBuilder SetResultNode(TerminalNode result)
    {
        _resultNode = result;
        return this;
    }

    public TerminalCommandBuilder SetQueryNode(TerminalNode query)
    {
        _queryNode = query;
        return this;
    }

    public TerminalCommandBuilder SetCancelNode(TerminalNode cancel)
    {
        _cancelNode = cancel;
        return this;
    }

    public TerminalCommandBuilder SetContinueWord(string word)
    {
        if (_continueWord != null)
        {
            UnityEngine.Object.Destroy(_continueWord);
        }

        _continueWord = ScriptableObject.CreateInstance<TerminalKeyword>();
        _continueWord.word = word;
        return this;
    }

    public TerminalCommandBuilder SetCancelWord(string word)
    {
        if (_cancelWord != null)
        {
            UnityEngine.Object.Destroy(_cancelWord);
        }

        _cancelWord = ScriptableObject.CreateInstance<TerminalKeyword>();
        _cancelWord.word = word;
        return this;
    }

    public TerminalCommandBuilder AddResultAction(Func<string> _func)
    {
        if (_resultNode != null)
        {
            _resultNode.SetNodeFunction(_func);
        }
        else
        {
            DawnPlugin.Logger.LogWarning("Unable to set result action for null TerminalNode!");
        }

        return this;
    }

    public TerminalCommandBuilder AddQueryAction(Func<string> _func)
    {
        if (_queryNode != null)
        {
            _queryNode.SetNodeFunction(_func);
        }
        else
        {
            DawnPlugin.Logger.LogWarning("Unable to set query action for null TerminalNode!");
        }

        return this;
    }

    public TerminalCommandBuilder AddCancelAction(Func<string> _func)
    {
        if (_cancelNode != null)
        {
            _cancelNode.SetNodeFunction(_func);
        }
        else
        {
            DawnPlugin.Logger.LogWarning("Unable to set cancel action for null TerminalNode!");
        }

        return this;
    }

    public void FinishBuild()
    {
        if (!ContinueBuild())
        {
            DawnPlugin.Logger.LogWarning("Unable to continue with the command build. You are missing a required component (result node or valid keywords)");
            return;
        }

        if (!IsQueryCommand())
        {
            AssignKeywordsToResultNode();
            Debuggers.Terminal?.Log($"Creating standard command [{_commandName}]");
        }
        else
        {
            AssignKeywordsToQueryNode();
            Debuggers.Terminal?.Log($"Creating query-style command [{_commandName}]");
        }

        List<TerminalKeyword> listing = [.. TerminalRefs.Instance.terminalNodes.allKeywords];
        listing.AddRange(_validKeywords);
        TerminalRefs.Instance.terminalNodes.allKeywords = [.. listing];
        if (!_resultNode.HasDawnInfo())
        {
            DawnPlugin.Logger.LogError($"No DawnInfo found on result node for command: {_commandName}!");
            return;
        }

        DawnTerminalCommandInfo commandInfo = _resultNode.GetDawnInfo();
        commandInfo.KeywordList = _validKeywords;

        if (commandInfo.QueryCommandInfo != null)
        {
            if (_queryNode == null || _cancelNode == null)
            {
                DawnPlugin.Logger.LogError($"No query node found for command: {_commandName} despite expecting it!");
                return;
            }

            commandInfo.QueryCommandInfo.QueryNode = _queryNode;
            commandInfo.QueryCommandInfo.CancelNode = _cancelNode;
        }
    }

    internal TerminalCommandBuilder AssignKeywordsToResultNode()
    {
        foreach (TerminalKeyword keyword in _validKeywords)
        {
            keyword.specialKeywordResult = _resultNode;
        }

        return this;
    }

    internal TerminalCommandBuilder AssignKeywordsToQueryNode()
    {
        if (_queryNode == null)
        {
            DawnPlugin.Logger.LogWarning($"Unable to assign keywords for [{_commandName}] to a NULL QueryNode!");
            return this;
        }

        foreach (TerminalKeyword keyword in _validKeywords)
        {
            keyword.specialKeywordResult = _queryNode;
            CompatibleNoun cont = new()
            {
                noun = _continueWord,
                result = _resultNode
            };

            CompatibleNoun cancel = new()
            {
                noun = _cancelWord,
                result = _cancelNode
            };

            _queryNode.terminalOptions = [cont, cancel];
            _queryNode.overrideOptions = true;
        }

        return this;
    }

    private bool ContinueBuild()
    {
        if (_resultNode == null || _validKeywords.Count == 0)
        {
            return false;
        }

        return true;
    }

    private bool IsQueryCommand()
    {
        return _queryNode != null && _cancelNode != null && _cancelWord != null && _continueWord != null;
    }
}
