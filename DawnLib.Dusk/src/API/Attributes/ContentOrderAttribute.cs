using System;

namespace Dusk;
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class ContentOrderAttribute(int order) : Attribute
{
    public int Order { get; } = order;
}