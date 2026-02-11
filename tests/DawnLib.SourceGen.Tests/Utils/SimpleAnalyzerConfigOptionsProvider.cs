using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Dawn.SourceGen.Tests.Utils;

class SimpleAnalyzerConfigOptionsProvider(AnalyzerConfigOptions global) : AnalyzerConfigOptionsProvider
{
    private readonly AnalyzerConfigOptions _global = global;

    public override AnalyzerConfigOptions GlobalOptions => _global;
    public override AnalyzerConfigOptions GetOptions(SyntaxTree tree) => _global;
    public override AnalyzerConfigOptions GetOptions(AdditionalText textFile) => _global;
}
