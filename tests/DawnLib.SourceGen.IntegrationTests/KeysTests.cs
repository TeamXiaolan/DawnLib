namespace Dawn.SourceGen.IntegrationTests;

#pragma warning disable IDE0002 // Don't "Simplify Member Access". Test the fully qualified path with namespace.
#pragma warning disable xUnit1047 // "Avoid using TheoryDataRow arguments that might not be serializable." NamespacedKey has ToString which is good enough.

public class KeysTests
{
    [Fact]
    public void MoonKeys()
    {
        var key = Dawn.SourceGen.IntegrationTests.MoonKeys.Test;
        Assert.True(key.IsVanilla());
        Assert.Equal("lethal_company:test", key.ToString());
        Assert.Equal(Dawn.MoonKeys.Test.ToString(), key.ToString());
    }

    [Theory]
    [MemberData(nameof(ReflectionData))]
    public void Reflection(string name, NamespacedKey? expectedKey)
    {
        var key = Dawn.SourceGen.IntegrationTests.MoonKeys.GetByReflection(name);
        Assert.Equal(expectedKey?.ToString(), key?.ToString());
    }

    public static IEnumerable<TheoryDataRow<string, NamespacedKey?>> ReflectionData =>
        [
            new("Test", Dawn.SourceGen.IntegrationTests.MoonKeys.Test),
            new("test", null),
            new("not found", null),
        ];
}
