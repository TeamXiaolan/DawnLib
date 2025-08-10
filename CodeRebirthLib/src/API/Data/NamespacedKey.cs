using System;
using BepInEx;

namespace CodeRebirthLib;
public class NamespacedKey
{
    public const char Separator = ':';
    public const string VanillaNamespace = "lethal_company";
    
    public string Namespace { get; }
    public string Key { get; }

    protected NamespacedKey(string @namespace, string key)
    {
        Namespace = @namespace;
        Key = key;
    }

    public static NamespacedKey From(string @namespace, string key)
    {
        return new NamespacedKey(@namespace, key);
    }

    public static NamespacedKey From<T>(string key) where T : BaseUnityPlugin
    {
        BepInPlugin plugin = MetadataHelper.GetMetadata(typeof(T));
        return From(plugin.Name, key);
    }

    public static NamespacedKey Vanilla(string key)
    {
        return From(VanillaNamespace, key);
    }

    public static NamespacedKey Parse(string input)
    {
        string[] parts = input.Split(Separator);
        return From(parts[0], parts[1]);
    }

    public override string ToString()
    {
        return $"{Namespace}{Separator}{Key}";
    }

    public override bool Equals(object? obj)
    {
        if (obj == null || GetType() != obj.GetType()) return false;
        NamespacedKey other = (NamespacedKey)obj;
        return Namespace == other.Namespace && Key == other.Key;
    }

    public override int GetHashCode()
    {
        // this should be like a proper hash implementation. womp womp
        return Namespace.GetHashCode() * 5 + Key.GetHashCode();
    }

    public NamespacedKey<T> AsTyped<T>() where T : INamespaced
    {
        return NamespacedKey<T>.From(Namespace, Key);
    }

    public bool IsVanilla() => Namespace == VanillaNamespace;
    public bool IsModded() => !IsVanilla();
}

// todo: is there anyway to not do the duplication for the From/Vanilla/Parse methods? or am i stuck with it because generics
public class NamespacedKey<T> : NamespacedKey where T : INamespaced
{
    protected NamespacedKey(string @namespace, string key) : base(@namespace, key) { }
    
    public new static NamespacedKey<T> From(string @namespace, string key)
    {
        return new NamespacedKey<T>(@namespace, key);
    }

    public new static NamespacedKey<T> From<TPlugin>(string key) where TPlugin : BaseUnityPlugin
    {
        BepInPlugin plugin = MetadataHelper.GetMetadata(typeof(TPlugin));
        return From(plugin.Name, key);
    }

    public new static NamespacedKey<T> Vanilla(string key)
    {
        return From(VanillaNamespace, key);
    }

    public new static NamespacedKey<T> Parse(string input)
    {
        string[] parts = input.Split(Separator);
        return From(parts[0], parts[1]);
    }
}