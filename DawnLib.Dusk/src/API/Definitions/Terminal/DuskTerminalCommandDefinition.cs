using System;
using System.Collections.Generic;
using Dawn;
using UnityEngine;

namespace Dusk;

[Serializable]
public class SimpleQueryCommand
{
    [field: SerializeField]
    public bool Enabled { get; private set; }

    [field: SerializeField]
    public string ResultDisplayText { get; private set; }

    [field: SerializeField]
    public string ContinueOrCancelDisplayText { get; private set; }
    [field: SerializeField]
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
    public List<string> ResultDisplayTexts { get; private set; }

    [field: SerializeField]
    public List<string> ContinueKeywords { get; private set; }

    [field: SerializeField]
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
    public List<string> ResultDisplayTexts { get; private set; }
}

[Serializable]
public class SimpleCommand
{
    [field: SerializeField]
    public bool Enabled { get; private set; }

    [field: SerializeField]
    public string ResultDisplayText { get; private set; }
}

[CreateAssetMenu(fileName = "New TerminalCommand Definition", menuName = $"{DuskModConstants.Definitions}/TerminalCommand Definition")]
public class DuskTerminalCommandDefinition : DuskContentDefinition<DawnTerminalCommandInfo>
{
    [field: SerializeField]
    public TerminalCommandBasicInformation CommandBasicInformation { get; private set; }

    [field: SerializeField]
    public List<string> CommandKeywordsList { get; private set; }

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

    public override void Register(DuskMod mod)
    {
        base.Register(mod);

        DawnLib.DefineTerminalCommand(TypedKey, CommandBasicInformation, builder =>
        {
            builder.SetKeywords(CommandKeywordsList);
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
            ApplyTagsTo(builder);
        });
    }

    public override void TryNetworkRegisterAssets()
    {

    }

    protected override string EntityNameReference => CommandBasicInformation.CommandName ?? string.Empty;
}