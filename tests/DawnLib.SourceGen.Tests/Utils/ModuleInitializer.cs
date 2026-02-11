using System.Runtime.CompilerServices;

namespace Dawn.SourceGen.Tests.Utils;

public static class ModuleInitializer
{
    [ModuleInitializer]
    public static void Init()
    {
        VerifySourceGenerators.Initialize();
    }
}
