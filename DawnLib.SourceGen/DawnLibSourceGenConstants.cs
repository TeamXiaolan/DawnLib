using Dawn;

namespace Dawn.SourceGen;
public static class DawnLibSourceGenConstants
{
    public const string CodeGenAttribute = $"""System.CodeDom.Compiler.GeneratedCode("DawnLib", "{MyPluginInfo.PLUGIN_VERSION}")""";
}