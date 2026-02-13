using System;
using InjectionLibrary.Attributes;

[assembly: RequiresInjections]

namespace Dawn;

[InjectInterface(typeof(TerminalNode))]
public interface ITerminalNode
{
    public Func<string> DynamicDisplayText { get; set; }
}
