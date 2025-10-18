using System;

namespace Dawn.Preloader;

[AttributeUsage(AttributeTargets.Interface, AllowMultiple = true)]
public sealed class InjectInterfaceAttribute(string typeName, string assemblyName = "Assembly-CSharp") : Attribute
{
    public string AssemblyName => assemblyName;
    public string TypeName => typeName;
}