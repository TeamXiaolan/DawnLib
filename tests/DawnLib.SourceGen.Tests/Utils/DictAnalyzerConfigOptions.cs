using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Dawn.SourceGen.Tests.Utils;

class DictAnalyzerConfigOptions(IDictionary<string, string> values) : AnalyzerConfigOptions
{
    private readonly ImmutableDictionary<string, string> _values = values.ToImmutableDictionary();

    public override bool TryGetValue(string key, [MaybeNullWhen(false)] out string value)
        => _values.TryGetValue(key, out value);
}
