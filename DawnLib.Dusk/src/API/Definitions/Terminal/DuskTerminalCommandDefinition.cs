using System;
using System.Collections.Generic;
using System.Text;
using Dawn;
using Dusk.Utils;
using UnityEngine;

namespace Dusk;

[Serializable]
public class SimpleQueryCommand
{
    [field: SerializeField]
    public bool Enabled { get; private set; }

    [field: SerializeField]
    [field: TextArea(3, 10)]
    public string ResultDisplayText { get; private set; }

    [field: SerializeField]
    [field: TextArea(3, 10)]
    public string ContinueOrCancelDisplayText { get; private set; }
    [field: SerializeField]
    [field: TextArea(3, 10)]
    public string CancelDisplayText { get; private set; }

    [field: SerializeField]
    public string CancelKeyword { get; private set; }

    [field: SerializeField]
    public string ContinueKeyword { get; private set; }
}

[Serializable]
public class ComplexQueryCommand
{
    [field: SerializeField]
    public bool Enabled { get; private set; }

    [field: SerializeField]
    [field: TextArea(3, 10)]
    public List<string> ResultDisplayTexts { get; private set; }

    [field: SerializeField]
    public List<string> ContinueKeywords { get; private set; }

    [field: SerializeField]
    [field: TextArea(3, 10)]
    public string ContinueOrCancelDisplayText { get; private set; }
}

[Serializable]
public class ComplexCommand
{
    [field: SerializeField]
    public bool Enabled { get; private set; }

    [field: SerializeField]
    public List<string> SecondaryKeywords { get; private set; }

    [field: SerializeField]
    [field: TextArea(3, 10)]
    public List<string> ResultDisplayTexts { get; private set; }
}

[Serializable]
public class SimpleCommand
{
    [field: SerializeField]
    public bool Enabled { get; private set; }

    [field: SerializeField]
    [field: TextArea(3, 10)]
    public string ResultDisplayText { get; private set; }
}

[Serializable]
public class SimpleInputCommand
{
    [field: SerializeField]
    public bool Enabled { get; private set; }

    [field: SerializeField]
    public List<string> Inputs { get; private set; }

    [field: SerializeField]
    [field: TextArea(3, 10)]
    public List<string> InputResults { get; private set; }

    [field: SerializeField]
    [field: TextArea(3, 10)]
    public string FailedInputResult { get; private set; }
}

[Serializable]
public class StoryLogCommand
{
    [field: SerializeField]
    public bool Enabled { get; private set; }

    [field: SerializeField]
    [field: Tooltip("Keywords such as sigurd `gold` or sigurd `work`, etc.")]
    public List<string> InputKeywords { get; private set; } = new();

    [field: SerializeField]
    [field: Tooltip("Result of each keyword in the same order as InputKeywords.")]
    [field: TextArea(2, 10)]
    public List<string> ResultDisplayTexts { get; private set; } = new();

    [field: SerializeField]
    [field: Tooltip("Keys that need to be saved in order to unlock the above entries.")]
    public List<NamespacedKey> SavedResultKeys { get; private set; } = new();
    [field: SerializeField]
    [field: Tooltip("Appends text for each key that is in the save file.")]
    public List<string> TextToAppendAfterEmptyResult { get; private set; } = new();
    [field: SerializeField]
    [field: Tooltip("Text added to the very beginning of the command when not given or given an invalid input.")]
    [field: TextArea(2, 10)]
    public string EmptyResultDisplayText { get; private set; }
    [field: SerializeField]
    [field: Tooltip("Text added to the very end of the command when not given or given an invalid input.")]
    [field: TextArea(2, 10)]
    public string PostEmptyResultDisplayText { get; private set; }

    internal string ResultDisplayText(string userInput)
    {
        PersistentDataContainer? save = DawnLib.GetCurrentSave();
        if (save == null)
        {
            return EmptyResultDisplayText;
        }

        HashSet<NamespacedKey> savedKeys = save.GetOrCreateDefault<HashSet<NamespacedKey>>(Dawn.Utils.ExtraScanEvents._dataKey);
        savedKeys.UnionWith(save.GetOrCreateDefault<HashSet<NamespacedKey>>(CommitKeyToSave.DawnLibLoreKey));
        for (int i = 0; i < InputKeywords.Count; i++)
        {
            if (!savedKeys.Contains(SavedResultKeys[i]))
                continue;

            if (userInput.Equals(InputKeywords[i], System.StringComparison.OrdinalIgnoreCase))
            {
                return ResultDisplayTexts[i];
            }
        }

        StringBuilder stringBuilder = new(EmptyResultDisplayText);
        for (int i = 0; i < InputKeywords.Count; i++)
        {
            if (!savedKeys.Contains(SavedResultKeys[i]))
                continue;

            stringBuilder.Append(TextToAppendAfterEmptyResult[i]);
        }

        stringBuilder.Append(PostEmptyResultDisplayText);

        return stringBuilder.ToString();
    }
}

[CreateAssetMenu(fileName = "New TerminalCommand Definition", menuName = $"{DuskModConstants.Definitions}/TerminalCommand Definition")]
public class DuskTerminalCommandDefinition : DuskContentDefinition<DawnTerminalCommandInfo>
{
    [field: SerializeField]
    public TerminalCommandBasicInformation CommandBasicInformation { get; private set; }

    [field: SerializeField]
    public List<string> CommandKeywordsList { get; private set; }

    [field: SerializeField]
    [field: Tooltip("Your own sigurd-like storylog command, you'd need to implement gathering the entries in your own way, using Components like ExtraScanEvents and CommitKeyToSave.")]
    public StoryLogCommand StoryLogCommand { get; private set; }
    [field: SerializeField]
    [field: Tooltip("Setup a query command that involves a continue and cancel operation to reach the main command text.")]
    public SimpleQueryCommand SimpleQueryCommand { get; private set; }

    [field: SerializeField]
    [field: Tooltip("Setup a complex query command that can have multiple continues and one cancel operation to reach multiple main command texts.")]
    public ComplexQueryCommand ComplexQueryCommand { get; private set; }

    [field: SerializeField]
    public ComplexCommand ComplexCommand { get; private set; }

    [field: SerializeField]
    public SimpleCommand SimpleCommand { get; private set; }

    [field: SerializeField]
    public SimpleInputCommand SimpleInputCommand { get; private set; }

    public override void Register(DuskMod mod)
    {
        base.Register(mod);
        DawnLib.DefineTerminalCommand(TypedKey, CommandBasicInformation, builder =>
        {
            builder.SetKeywords(CommandKeywordsList);
            if (StoryLogCommand.Enabled)
            {
                builder.DefineInputCommand(inputCommandBuilder =>
                {
                    inputCommandBuilder.SetResultDisplayText(StoryLogCommand.ResultDisplayText);
                });
            }

            if (SimpleQueryCommand.Enabled)
            {
                builder.DefineSimpleQueryCommand(simpleQueryBuilder =>
                {
                    simpleQueryBuilder.SetResult(() => SimpleQueryCommand.ResultDisplayText);
                    simpleQueryBuilder.SetCancel(() => SimpleQueryCommand.CancelDisplayText);
                    simpleQueryBuilder.SetContinueOrCancel(() => SimpleQueryCommand.ContinueOrCancelDisplayText);
                    simpleQueryBuilder.SetCancelWord(SimpleQueryCommand.CancelKeyword);
                    simpleQueryBuilder.SetContinueWord(SimpleQueryCommand.ContinueKeyword);
                });
            }

            if (ComplexQueryCommand.Enabled)
            {
                builder.DefineComplexQueryCommand(complexQueryBuilder =>
                {
                    List<Func<string>> resultDisplayTexts = new();
                    foreach (string resultDisplayText in ComplexQueryCommand.ResultDisplayTexts)
                    {
                        resultDisplayTexts.Add(() => resultDisplayText);
                    }
                    complexQueryBuilder.SetResultDisplayTexts(resultDisplayTexts);
                    complexQueryBuilder.SetContinueOrCancelDisplayTexts(() => ComplexQueryCommand.ContinueOrCancelDisplayText);
                    complexQueryBuilder.SetContinueKeywords(ComplexQueryCommand.ContinueKeywords);
                });
            }

            if (ComplexCommand.Enabled)
            {
                builder.DefineComplexCommand(complexCommandBuilder =>
                {
                    complexCommandBuilder.SetSecondaryKeywords(ComplexCommand.SecondaryKeywords);

                    List<Func<string>> resultDisplayTexts = new();
                    foreach (string resultDisplayText in ComplexCommand.ResultDisplayTexts)
                    {
                        resultDisplayTexts.Add(() => resultDisplayText);
                    }
                    complexCommandBuilder.SetResultsDisplayText(resultDisplayTexts);
                });
            }

            if (SimpleCommand.Enabled)
            {
                builder.DefineSimpleCommand(simpleCommandBuilder =>
                {
                    simpleCommandBuilder.SetResultDisplayText(() => SimpleCommand.ResultDisplayText);
                });
            }

            if (SimpleInputCommand.Enabled)
            {
                builder.DefineInputCommand(simpleInputCommandBuilder =>
                {
                    simpleInputCommandBuilder.SetResultDisplayText(SimpleInputResult);
                });
            }
            ApplyTagsTo(builder);
        });
    }

    public string SimpleInputResult(string input)
    {
        for (int i = 0; i < SimpleInputCommand.Inputs.Count; i++)
        {
            string possibleInput = SimpleInputCommand.Inputs[i];
            if (input.Equals(possibleInput, StringComparison.OrdinalIgnoreCase))
            {
                return SimpleInputCommand.InputResults[i];
            }
        }
        return SimpleInputCommand.FailedInputResult;
    }

    public override void TryNetworkRegisterAssets()
    {

    }

    protected override string EntityNameReference => CommandBasicInformation.CommandName ?? string.Empty;
}