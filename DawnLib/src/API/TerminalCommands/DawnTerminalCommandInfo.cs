using System.Collections.Generic;

namespace Dawn;
public sealed class DawnTerminalCommandInfo : DawnBaseInfo<DawnTerminalCommandInfo>
{
    internal DawnTerminalCommandInfo(NamespacedKey<DawnTerminalCommandInfo> key, TerminalNode resultNode, HashSet<NamespacedKey> tags, DawnQueryCommandInfo? queryCommandInfo, IDataContainer? customData) : base(key, tags, customData)
    {
        ResultNode = resultNode;

        QueryCommandInfo = queryCommandInfo;
        if (QueryCommandInfo != null) QueryCommandInfo.ParentInfo = this;
        SimpleCommandInfo = simpleCommandInfo; // keyword with a special keyword result
        if (SimpleCommandInfo != null) SimpleCommandInfo.ParentInfo = this;
    }

    public TerminalNode ResultNode { get; }
    public List<TerminalKeyword>? KeywordList { get; internal set; }

    public DawnQueryCommandInfo? QueryCommandInfo { get; private set; }
}