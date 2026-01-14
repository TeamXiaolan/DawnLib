using System.Collections.Generic;

namespace Dawn.SourceGen.AST;

public class GeneratedEnum(string visibility, string name) : IClassMember, IAttributeContainer, ITopLevelSymbol
{
    public class EnumValue(string name, string? value = null) : IAttributeContainer
    {
        public string Name { get; } = name;
        public string? Value { get; } = value;
        public List<string> Attributes { get; } = [];
    }

    public string Visibility = visibility;
    public string Name = name;

    public List<EnumValue> Values { get; } = [];

    public void Visit(ISymbolVisitor visitor)
    {
        visitor.Accept(this);
    }

    public List<string> Signature()
    {
        List<string> flags = [Visibility, "enum", Name];
        return flags;
    }

    public List<string> Attributes { get; } = [];
}