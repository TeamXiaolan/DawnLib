using System.Collections.Generic;

namespace Dawn.SourceGen.AST;

public class GeneratedField(string visibility, string type, string name) : IClassMember, IAttributeContainer
{
    public string Visibility = visibility;

    public string Type = type;
    public string Name = name;

    public bool IsStatic;

    public string? Value;

    public List<string> Signature()
    {
        List<string> flags = [Visibility];

        if (IsStatic) flags.Add("static");

        flags.Add(Type);
        flags.Add($"{Name}");
        return flags;
    }

    public void Visit(ISymbolVisitor visitor)
    {
        visitor.Accept(this);
    }

    public List<string> Attributes { get; } = [];
}