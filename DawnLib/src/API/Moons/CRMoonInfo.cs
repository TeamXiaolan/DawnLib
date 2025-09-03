using System.Collections.Generic;

namespace CodeRebirthLib;

public class CRMoonInfo : CRBaseInfo<CRMoonInfo>
{
    internal CRMoonInfo(NamespacedKey<CRMoonInfo> key, List<NamespacedKey> tags, SelectableLevel level, TerminalNode? routeNode, TerminalKeyword? nameKeyword) : base(key, tags)
    {
        Level = level;
        RouteNode = routeNode;
        NameKeyword = nameKeyword;
    }

    public SelectableLevel Level { get; }
    public TerminalNode? RouteNode { get; }
    public TerminalKeyword? NameKeyword { get; }
}