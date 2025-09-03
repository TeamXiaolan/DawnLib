using Microsoft.CodeAnalysis;

namespace Dawn.SourceGen;
public static class DawnLibDiagnostics
{
    public static readonly DiagnosticDescriptor MissingRootNamespace = new DiagnosticDescriptor(
        id: "DAWN001",
        title: "Missing Root Namespace",
        messageFormat: "The project does not define a RootNamespace. Please set <RootNamespace> in your .csproj.",
        category: "SourceGen",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );
}