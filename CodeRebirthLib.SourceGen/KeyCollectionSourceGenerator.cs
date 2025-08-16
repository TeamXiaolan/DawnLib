using System;
using System.Collections.Generic;
using System.Text;
using CodeRebirthLib.SourceGen.AST;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Newtonsoft.Json;

namespace CodeRebirthLib.SourceGen;
[Generator]
public class KeyCollectionSourceGenerator : ISourceGenerator
{
    const string CodeGenAttribute = $"""System.CodeDom.Compiler.GeneratedCode("CodeRebirthLib", "{MyPluginInfo.PLUGIN_VERSION}")""";
    
    public void Initialize(GeneratorInitializationContext context)
    {
        
    }
    public void Execute(GeneratorExecutionContext context)
    {
        foreach (AdditionalText? additionalFile in context.AdditionalFiles)
        {
            if (additionalFile == null)
                continue;

            if (!additionalFile.Path.EndsWith("namespaced_keys.json"))
                continue;

            SourceText? text = additionalFile.GetText();
            if (text == null)
                continue;

            Dictionary<string, Dictionary<string, string>> definitions = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, string>>>(text.ToString())!;

            foreach (string className in definitions.Keys)
            {
                Dictionary<string, string> values = definitions[className];
                GeneratedClass @class = new GeneratedClass(Visibility.Public, className)
                {
                    IsStatic = true,
                    Attributes = { CodeGenAttribute }
                };
                string type = $"NamespacedKey<{values["__type"]}>";

                foreach (var value in values)
                {
                    if(value.Key == "__type") continue;
                    string[] parts = value.Value.Split(':');

                    GeneratedField field = new GeneratedField(Visibility.Public, type, value.Key)
                    {
                        IsStatic = true
                    };

                    if (parts[0] == "lethal_company")
                    {
                        field.Value = $"{type}.Vanilla(\"{parts[1]}\")";
                    }
                    else
                    {
                        field.Value = $"{type}.From(\"{parts[0]}\",\"{parts[1]}\")";
                    }
                    @class.Members.Add(field);
                }

                GeneratedCodeFile file = new GeneratedCodeFile()
                {
                    Namespace = "CodeRebirthLib", // todo
                    Usings = ["CodeRebirthLib"],
                    Symbols = [ @class ]
                };

                FileWriterVisitor visitor = new FileWriterVisitor();
                visitor.Accept(file);
                
                context.AddSource($"{className}.g.cs", SourceText.From(visitor.ToString(), Encoding.UTF8));
            }
        }
    }
}