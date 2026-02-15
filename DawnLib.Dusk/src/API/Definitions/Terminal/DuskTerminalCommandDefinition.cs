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

[CreateAssetMenu(fileName = "New TerminalCommand Definition", menuName = $"{DuskModConstants.Definitions}/TerminalCommand Definition")]
public class DuskTerminalCommandDefinition : DuskContentDefinition<DawnTerminalCommandInfo>
{
    [field: SerializeField]
    public TerminalCommandBasicInformation CommandBasicInformation { get; private set; }

    [field: SerializeField]
    public List<string> CommandKeywordsList { get; private set; }

    [field: SerializeField]
    [field: Tooltip("Setup a query command that involves a continue and cancel operation to reach the main command text.")]
    public SimpleQueryCommand QueryCommand { get; private set; }

    public override void Register(DuskMod mod)
    {
        base.Register(mod);

        DawnLib.DefineTerminalCommand(TypedKey, CommandBasicInformation, builder =>
        {
            builder.SetKeywords(CommandKeywordsList);
            if (QueryCommand.Enabled)
            {
                builder.DefineSimpleQueryCommand(builder =>
                {
                    builder.SetResult(() => QueryCommand.ResultDisplayText);
                    builder.SetCancel(() => QueryCommand.CancelDisplayText);
                    builder.SetContinueOrCancel(() => QueryCommand.ContinueOrCancelDisplayText);
                    builder.SetCancelWord(QueryCommand.CancelKeyword);
                    builder.SetContinueWord(QueryCommand.ContinueKeyword);
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