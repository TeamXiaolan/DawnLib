namespace Dawn.SourceGen.AST;

public interface ISymbol {
	void Visit(ISymbolVisitor visitor);
}