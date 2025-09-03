using System.Text;
using Dawn.SourceGen.Extensions;

namespace Dawn.SourceGen.AST;

public class FileWriterVisitor : ISymbolVisitor {
	int _currentIndent;

	StringBuilder _builder = new StringBuilder();

	void AppendLine(string line) {
		for(int i = 0; i < _currentIndent; i++) {
			_builder.Append("\t");
		}

		_builder.AppendLine(line);
	}

	void AppendAttributes(IAttributeContainer attributeContainer) {
		foreach(string attribute in attributeContainer.Attributes) {
			AppendLine($"[{attribute}]");
		}
	}
	
	public void Accept(GeneratedClass @class) {
		StringBuilder definition = new StringBuilder();
		AppendAttributes(@class);

		definition.Append(string.Join(" ", @class.Signature()));
		if(!string.IsNullOrEmpty(@class.DefaultConstructor))
			definition.Append($"({@class.DefaultConstructor})");

		if(@class.Implements.Count > 0) {
			definition.Append(" : ");
			definition.Append(string.Join(", ", @class.Implements));
		}

		AppendLine(definition + " {");
		_currentIndent++;
		foreach(IClassMember member in @class.Members) {
			member.Visit(this);
		}

		_currentIndent--;
		AppendLine("}");
	}

	public void Accept(GeneratedEnum @enum) {
		StringBuilder builder = new StringBuilder();
		AppendAttributes(@enum);
		builder.Append(string.Join(" ", @enum.Signature()));
		
		AppendLine(builder + " {");
		_currentIndent++;

		builder.Clear();
		foreach((int i, GeneratedEnum.EnumValue value) in @enum.Values.WithIndex()) {
			AppendAttributes(value);
			builder.Append(value.Name);

			if(value.Value != null) {
				builder.Append($" = {value.Value}");
			}

			if(i != @enum.Values.Count - 1) {
				builder.Append(",");
			}
			
			AppendLine(builder.ToString());
			builder.Clear();
		}
		
		_currentIndent--;
		AppendLine("}");
	}

	public void Accept(GeneratedCodeFile codeFile) {
		foreach(string @using in codeFile.Usings) {
			_builder.AppendLine($"using {@using};");
		}

		if(!string.IsNullOrEmpty(codeFile.Namespace))
			_builder.AppendLine($"namespace {codeFile.Namespace};");

		foreach(ITopLevelSymbol symbol in codeFile.Symbols) {
			symbol.Visit(this);
		}
	}

	public void Accept(GeneratedField field) {
		StringBuilder builder = new StringBuilder();
		AppendAttributes(field);
		
		builder.Append(string.Join(" ", field.Signature()));
		if(field.Value != null) {
			builder.Append($" = {field.Value}");
		}

		builder.Append(";");
		AppendLine(builder.ToString());
	}

	public void Accept(GeneratedMethod method) {
		AppendAttributes(method);
		StringBuilder definition = new StringBuilder();
		definition.Append(string.Join(" ", method.Signature()));
		definition.Append("(");
		definition.Append(string.Join(", ", method.Params));
		AppendLine(definition + ") {");
		_currentIndent++;
		foreach(string line in method.Body) {
			AppendLine(line);
		}

		_currentIndent--;
		AppendLine("}");
	}

	public override string ToString() {
		return _builder.ToString();
	}
}