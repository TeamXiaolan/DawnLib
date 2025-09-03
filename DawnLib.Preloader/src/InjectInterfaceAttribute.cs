using System;

namespace Dawn.Preloader;

[AttributeUsage(AttributeTargets.Interface, AllowMultiple = true)]
public sealed class InjectInterfaceAttribute(string fullName) : Attribute
{
    public string FullName => fullName;
}