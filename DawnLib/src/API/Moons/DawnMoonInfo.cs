using System.Collections.Generic;

namespace Dawn;

public class DawnMoonInfo : DawnBaseInfo<DawnMoonInfo>
{
    internal DawnMoonInfo(NamespacedKey<DawnMoonInfo> key, List<NamespacedKey> tags, SelectableLevel level, TerminalNode? routeNode, TerminalKeyword? nameKeyword) : base(key, tags)
    {
        Level = level;
        RouteNode = routeNode;
        NameKeyword = nameKeyword;
    }

    public SelectableLevel Level { get; }
    public TerminalNode? RouteNode { get; }
    public TerminalKeyword? NameKeyword { get; }
}