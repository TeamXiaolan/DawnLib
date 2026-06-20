using System;

namespace Dawn;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class LoadingStepHardDependencyAttribute(string @namespace, string key) : Attribute
{
    public NamespacedKey Dependency { get; } = NamespacedKey.From(@namespace, key);
}

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class LoadingStepSoftDependencyAttribute(string @namespace, string key) : Attribute
{
    public NamespacedKey Dependency { get; } = NamespacedKey.From(@namespace, key);
}