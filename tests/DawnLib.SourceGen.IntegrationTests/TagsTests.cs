namespace Dawn.SourceGen.IntegrationTests;

#pragma warning disable IDE0002 // Don't "Simplify Member Access". Test the fully qualified path with namespace.

public class TagsTests
{
    [Fact]
    public void VanillaTopLevelTags()
    {
        var tag = Dawn.SourceGen.IntegrationTests.Tags.Hot;
        Assert.True(tag.IsVanilla());
        Assert.Equal("lethal_company:hot", tag.ToString());
        Assert.Equal(Dawn.Tags.Hot.ToString(), tag.ToString());
    }

    [Fact]
    public void VanillaSubdirTags()
    {
        var tag = Dawn.SourceGen.IntegrationTests.Tags.Artificial;
        Assert.True(tag.IsVanilla());
        Assert.Equal("lethal_company:artifical", tag.ToString());
        Assert.Equal(Dawn.Tags.Artificial.ToString(), tag.ToString());
    }

    [Fact]
    public void CustomTags()
    {
        var tag = Dawn.SourceGen.IntegrationTests.Tags.UnusualThings;
        Assert.True(tag.IsModded());
        Assert.Equal("custom_namespace:bright_light", tag.ToString());
    }
}
