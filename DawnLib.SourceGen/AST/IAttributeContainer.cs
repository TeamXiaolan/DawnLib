using System.Collections.Generic;

namespace Dawn.SourceGen.AST;

public interface IAttributeContainer
{
    public List<string> Attributes { get; }
}