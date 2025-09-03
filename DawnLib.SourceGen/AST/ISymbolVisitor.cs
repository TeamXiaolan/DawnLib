namespace Dawn.SourceGen.AST;

public interface ISymbolVisitor
{
    void Accept(GeneratedClass @class);
    void Accept(GeneratedCodeFile codeFile);
    void Accept(GeneratedField field);
    void Accept(GeneratedMethod method);
    void Accept(GeneratedEnum @enum);
}