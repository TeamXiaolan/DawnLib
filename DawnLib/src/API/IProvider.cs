using BepInEx.Configuration;
using System;

namespace Dawn;
public interface IProvider<out T>
{
    T Provide();
}

public class SimpleProvider<T>(T value) : IProvider<T>
{
    public T Provide() => value;
}

public class ConfigEntryProvider<T>(ConfigEntry<T> configEntry) : IProvider<T>
{
    public T Provide() => configEntry.Value;
}

public class FuncProvider<T>(Func<T> function) : IProvider<T>
{
    public T Provide() => function();
}