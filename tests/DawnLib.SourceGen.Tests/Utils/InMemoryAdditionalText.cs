using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Dawn.SourceGen.Tests.Utils;

internal class InMemoryAdditionalText(string path, string content) : AdditionalText
{
    public override string Path { get; } = path;

    private readonly SourceText _content = SourceText.From(content, Encoding.UTF8);

    public override SourceText GetText(CancellationToken cancellationToken = default)
        => _content;
}
