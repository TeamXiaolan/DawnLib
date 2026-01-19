using Dawn;
using UnityEngine;

namespace Dusk;

[CreateAssetMenu(fileName = "New TerminalCommand Definition", menuName = $"{DuskModConstants.Definitions}/TerminalCommand Definition")]
public class DuskTerminalCommandDefinition : DuskContentDefinition<DawnTerminalCommandInfo>
{
    [field: SerializeField]
    public string CommandName { get; private set; }

    public TerminalCommandConfig Config { get; private set; }

    public override void Register(DuskMod mod)
    {
        base.Register(mod);
        using ConfigContext section = mod.ConfigManager.CreateConfigSectionForBundleData(AssetBundleData);
        Config = CreateTerminalCommandConfig(section);

        DawnLib.DefineTerminalCommand(TypedKey, CommandName, builder =>
        {
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