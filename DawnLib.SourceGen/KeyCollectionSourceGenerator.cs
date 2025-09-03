using System.Collections.Generic;
using System.IO;
using System.Text;
using Dawn.SourceGen.AST;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Newtonsoft.Json;

namespace Dawn.SourceGen;
[Generator]
public class KeyCollectionSourceGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {
        
    }

    public void Execute(GeneratorExecutionContext context)
    {
        if (!context.AnalyzerConfigOptions.GlobalOptions.TryGetValue("build_property.rootnamespace", out string? rootNamespace) || string.IsNullOrWhiteSpace(rootNamespace))
        {
            context.ReportDiagnostic(Diagnostic.Create(CRLibDiagnostics.MissingRootNamespace, Location.None));
            return;
        }

        List<string> alreadyGenerated = [];
        
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
                    IsPartial = true
                };
                string type = $"NamespacedKey<{values["__type"]}>";

                foreach (var value in values)
                {
                    if (value.Key == "__type") continue;
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

                if (!alreadyGenerated.Contains(@class.Name))
                {
                    GeneratedMethod getReflectionMethod = new GeneratedMethod(Visibility.Public, $"{type}?", "GetByReflection")
                    {
                        IsStatic = true,
                        Params = ["string name"],
                        Body =
                        [
                            $"return ({type}?)typeof({@class.Name}).GetField(name)?.GetValue(null);"
                        ]
                    };
                    @class.Members.Add(getReflectionMethod);
                    @class.Attributes.Add(CRLibSourceGenConstants.CodeGenAttribute);
                }

                GeneratedCodeFile file = new GeneratedCodeFile()
                {
                    Namespace = rootNamespace,
                    Usings = ["Dawn"],
                    Symbols = [@class]
                };

                FileWriterVisitor visitor = new FileWriterVisitor();
                visitor.Accept(file);

                alreadyGenerated.Add(@class.Name);
                context.AddSource($"{Path.GetFileNameWithoutExtension(additionalFile.Path).Split('.')[0]}.{className}.g.cs", SourceText.From(visitor.ToString(), Encoding.UTF8));
            }
        }
    }
}