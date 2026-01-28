using System;
using System.Collections.Generic;
using Dawn;
using UnityEngine;
using static Dawn.TerminalCommandRegistration;

namespace Dusk;

[Serializable]
public class QueryCommand
{
    [field: SerializeField]
    public bool Enabled { get; private set; }

    [field: SerializeField]
    public string QueryDisplayText { get; private set; }

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
    public string CommandName { get; private set; }

    [field: SerializeField]
    [field: TextArea(2, 20)]
    public string CommandMainText { get; private set; }

    [field: SerializeField]
    public List<string> CommandKeywordsList { get; private set; }

    [field: SerializeField]
    [field: Tooltip("Category of the command for internal use.")]
    public string CommandCategory { get; private set; }

    [field: SerializeField]
    [field: Tooltip("Description of the command for internal use.")]
    public string CommandDescription { get; private set; }

    [field: SerializeField]
    [field: Tooltip("Clear text flags for the command, i.e. which commands clear text on entering.")]
    public ClearText CommandClearTextFlags { get; private set; }

    [field: SerializeField]
    [field: Tooltip("Setup a query command that involves a continue and cancel operation to reach the main command text.")]
    public QueryCommand QueryCommand { get; private set; }

    public TerminalCommandConfig Config { get; private set; }

    public override void Register(DuskMod mod)
    {
        base.Register(mod);
        using ConfigContext section = mod.ConfigManager.CreateConfigSectionForBundleData(AssetBundleData);
        Config = CreateTerminalCommandConfig(section);

        DawnLib.DefineTerminalCommand(TypedKey, CommandName, builder =>
        {
            builder.SetEnabled(new SimpleProvider<bool>(true));
            builder.SetMainText(() => CommandMainText);
            builder.SetKeywords(new SimpleProvider<List<string>>(CommandKeywordsList));
            builder.SetCategoryName(CommandCategory);
            builder.SetDescription(CommandDescription);
            builder.SetClearTextFlags(CommandClearTextFlags);
            if (QueryCommand.Enabled)
            {
                builder.DefineQueryCommand(builder =>
                {
                    builder.SetQuery(() => QueryCommand.QueryDisplayText);
                    builder.SetCancel(() => QueryCommand.CancelDisplayText);
                    builder.SetCancelWord(QueryCommand.CancelKeyword);
                    builder.SetContinueWord(QueryCommand.ContinueKeyword);
                });
            }
            ApplyTagsTo(builder);
        });
    }

    public TerminalCommandConfig CreateTerminalCommandConfig(ConfigContext section)
    {
        return new TerminalCommandConfig
        {
        };
    }

    public override void TryNetworkRegisterAssets()
    {

    }

    protected override string EntityNameReference => CommandName ?? string.Empty;
}