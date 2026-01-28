using System;
using Dawn;

namespace Dusk;

[Serializable]
public class DuskTerminalCommandReference : DuskContentReference<DuskTerminalCommandDefinition, DawnTerminalCommandInfo>
{
    public DuskTerminalCommandReference() : base()
    { }

    public DuskTerminalCommandReference(NamespacedKey<DawnTerminalCommandInfo> key) : base(key)
    { }

    public override bool TryResolve(out DawnTerminalCommandInfo info)
    {
        return LethalContent.TerminalCommands.TryGetValue(TypedKey, out info);
    }

    public override DawnTerminalCommandInfo Resolve()
    {
        return LethalContent.TerminalCommands[TypedKey];
    }
}