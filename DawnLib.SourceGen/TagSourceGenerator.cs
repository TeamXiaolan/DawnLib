using System.IO;
using System.Linq;
using System.Text;
using Dawn.SourceGen.AST;
using Dawn.SourceGen.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Newtonsoft.Json;

namespace Dawn.SourceGen;

[Generator]
public class TagSourceGenerator : ISourceGenerator
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
        
        GeneratedClass @class = new GeneratedClass(Visibility.Public, "Tags") // todo: e.g. MeltdownTags
        {
            IsStatic = true,
            Attributes = { CRLibSourceGenConstants.CodeGenAttribute }
        };
        
        foreach (AdditionalText? additionalFile in context.AdditionalFiles)
        {
            if (additionalFile == null)
                continue;

            if (!additionalFile.Path.EndsWith("tag.json"))
                continue;

            SourceText? text = additionalFile.GetText();
            if (text == null)
                continue;

            string fieldName = Path.GetFileName(additionalFile.Path).Split('.')[0];
            fieldName = string.Join("",fieldName.Split('_').Select(it => it.ToCapitalized()));
            
            TagDefinition definition = JsonConvert.DeserializeObject<TagDefinition>(text.ToString())!;
            string[] parts = definition.Tag.Split(':');
            GeneratedField field = new GeneratedField(Visibility.Public, "NamespacedKey", fieldName)
            {
                IsStatic = true
            };
            
            if (parts[0] == "lethal_company")
            {
                field.Value = $"NamespacedKey.Vanilla(\"{parts[1]}\")";
            }
            else
            {
                field.Value = $"NamespacedKey.From(\"{parts[0]}\",\"{parts[1]}\")";
            }
            @class.Members.Add(field);
        }

        if (@class.Members.Count == 0)
        {
            // don't generate tags class if there are no tags.
            return;
        }
        
        GeneratedCodeFile file = new GeneratedCodeFile()
        {
            Namespace = rootNamespace,
            Usings = ["CodeRebirthLib"],
            Symbols = [@class]
        };

        FileWriterVisitor visitor = new FileWriterVisitor();
        visitor.Accept(file);
        
        context.AddSource($"{@class.Name}.g.cs", SourceText.From(visitor.ToString(), Encoding.UTF8));
    }
}