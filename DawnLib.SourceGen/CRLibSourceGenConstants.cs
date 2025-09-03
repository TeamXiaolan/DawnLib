using CodeRebirthLib;

namespace Dawn.SourceGen;
public static class CRLibSourceGenConstants
{
    public const string CodeGenAttribute = $"""System.CodeDom.Compiler.GeneratedCode("CodeRebirthLib", "{MyPluginInfo.PLUGIN_VERSION}")""";
}