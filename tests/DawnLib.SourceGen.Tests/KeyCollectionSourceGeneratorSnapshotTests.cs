using Dawn.SourceGen;
using Microsoft.CodeAnalysis.CSharp;
using Dawn.SourceGen.Tests.Utils;

namespace Dawn.SourceGen.Tests;

public class KeyCollectionSourceGeneratorSnapshotTests
{
    [Fact]
    public Task KeyCollectionTest()
    {
        var text = new InMemoryAdditionalText("some_test_filename.namespaced_keys.json", """
{
  "EnemyKeys": {
    "__type": "TestEnemyInfo",
    "Blob": "lethal_company:blob"
  },
  "UnlockableItemKeys": {
    "__type": "TestUnlockableItemInfo",
    "Orangesuit": "lethal_company:orange_suit",
    "Fridge": "lethal_company:fridge",
    "NonVanillaNamespace": "something:else"
  }
}
""");
        var compilation = CSharpCompilation.Create(nameof(KeyCollectionTest));
        var generator = new KeyCollectionSourceGenerator();

        var globalValues = new Dictionary<string,string>
        {
            // Corresponds to <RootNamespace> in .csproj
            ["build_property.rootnamespace"] = "My.Test.Namespace",
        };
        var provider = new SimpleAnalyzerConfigOptionsProvider(new DictAnalyzerConfigOptions(globalValues));
        var driver = CSharpGeneratorDriver.Create(generator)
            .AddAdditionalTexts([text])
            .WithUpdatedAnalyzerConfigOptions(provider)
            .RunGenerators(compilation, TestContext.Current.CancellationToken);
        return Verify(driver, Settings.Instance);
    }

    [Fact]
    public Task DiagnosticDAWN001Test()
    {
        var compilation = CSharpCompilation.Create(nameof(DiagnosticDAWN001Test));
        var generator = new KeyCollectionSourceGenerator();
        var driver = CSharpGeneratorDriver.Create(generator)
            .RunGenerators(compilation, TestContext.Current.CancellationToken);
        return Verify(driver, Settings.Instance);
    }
}
