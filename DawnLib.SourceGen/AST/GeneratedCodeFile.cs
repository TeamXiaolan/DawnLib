using System.Collections.Generic;

namespace Dawn.SourceGen.AST;

public class GeneratedCodeFile : ISymbol {
	public string Namespace;

	public List<string> Usings = [];
	public List<ITopLevelSymbol> Symbols = [];
	
	public void Visit(ISymbolVisitor visitor) {
		visitor.Accept(this);
	}
}