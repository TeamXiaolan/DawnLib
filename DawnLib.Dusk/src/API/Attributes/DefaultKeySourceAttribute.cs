using System;
using UnityEngine;

namespace Dusk;
[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
public sealed class DefaultKeySourceAttribute(string memberName, bool normalize = true) : PropertyAttribute
{
    /// <summary>
    /// Field, property, or method name on the target object to provide the default key.
    /// If a method is used, it can be:
    ///   string GetDefaultForX();
    ///   string GetDefaultForX(int index); // index of element in list
    /// </summary>
    public string MemberName { get; } = memberName;

    /// <summary>
    /// If true (default), the drawer will run normalization on the string.
    /// </summary>
    public bool Normalize { get; } = normalize;
}
