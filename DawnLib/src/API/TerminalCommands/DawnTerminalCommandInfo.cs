using System.Collections.Generic;

namespace Dawn;
public sealed class DawnTerminalCommandInfo : DawnBaseInfo<DawnTerminalCommandInfo>
{
    internal DawnTerminalCommandInfo(NamespacedKey<DawnTerminalCommandInfo> key, HashSet<NamespacedKey> tags, TerminalNode result, DawnQueryCommandInfo? queryCommandInfo, IDataContainer? customData) : base(key, tags, customData)
    {
        ResultNode = result;
        QueryCommandInfo = queryCommandInfo;
        if (QueryCommandInfo != null) QueryCommandInfo.ParentInfo = this;
    }

    public TerminalNode ResultNode { get; set; }
    public List<TerminalKeyword>? KeywordList { get; internal set; }

    public DawnQueryCommandInfo? QueryCommandInfo { get; private set; }
}