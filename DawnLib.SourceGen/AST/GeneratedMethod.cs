using System.Collections.Generic;

namespace CodeRebirthLib.SourceGen.AST;

public class GeneratedMethod(string visibility, string returnType, string name) : IClassMember, IAttributeContainer {
	public string Visibility = visibility;
	
	public string ReturnType = returnType;
	public string Name = name;
	public List<string> Body = [];
	public List<string> Params = [];

	public bool IsStatic;
	public bool IsAsync;

	public List<string> Signature() {
		List<string> flags = [Visibility];

		if(IsStatic) flags.Add("static");
		if(IsAsync) flags.Add("async");

		flags.Add(ReturnType);
		flags.Add($"{Name}");
		return flags;
	}

	public void Visit(ISymbolVisitor visitor) {
		visitor.Accept(this);
	}

	public List<string> Attributes { get; } = [];
}