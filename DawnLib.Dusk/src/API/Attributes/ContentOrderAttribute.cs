using System;

namespace Dusk;
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
sealed class ContentOrderAttribute(int order) : Attribute
{
    public int Order { get; } = order;
}