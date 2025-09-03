using System.Collections.Generic;

namespace CodeRebirthLib.SourceGen.AST;

public interface IAttributeContainer {
	public List<string> Attributes { get; }
}