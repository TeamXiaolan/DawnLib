using System.Collections.Generic;

namespace Dawn.SourceGen.AST;

public class GeneratedClass(string visibility, string name) : IClassMember, IAttributeContainer, ITopLevelSymbol
{
    public string Visibility = visibility;
    public string Name = name;

    public bool IsPartial;
    public bool IsStatic;
    public bool IsSealed;

    public List<IClassMember> Members = [];
    public List<string> Implements = [];
    public string DefaultConstructor;

    public List<string> Attributes { get; } = [];

    public List<string> Signature()
    {
        List<string> flags = [Visibility];

        if (IsStatic) flags.Add("static");
        if (IsSealed) flags.Add("sealed");
        if (IsPartial) flags.Add("partial");

        flags.Add("class");
        flags.Add(Name);
        return flags;
    }

    public void Visit(ISymbolVisitor visitor)
    {
        visitor.Accept(this);
    }
}