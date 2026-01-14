using System;
using InjectionLibrary.Attributes;

[assembly: RequiresInjections]

namespace Dawn;

[InjectInterface(typeof(TerminalNode))]
public interface ITerminalNode
{
    //Used to update the displaytext of a node dynamically with additional logic, using Func<string>
    Func<string> DawnNodeFunction { get; set; }

    //method used to update node's displaytext from the NodeFunction property
    string GetDawnDisplayText();
}
