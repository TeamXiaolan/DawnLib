using System;

namespace Dawn;

public readonly struct WeightModifierHandle : IEquatable<WeightModifierHandle>
{
    private readonly Guid _id;

    private WeightModifierHandle(Guid id)
    {
        _id = id;
    }

    public static WeightModifierHandle New()
    {
        return new WeightModifierHandle(Guid.NewGuid());
    }

    public bool Equals(WeightModifierHandle other)
    {
        return _id.Equals(other._id);
    }

    public override bool Equals(object? obj)
    {
        return obj is WeightModifierHandle other && Equals(other);
    }

    public override int GetHashCode()
    {
        return _id.GetHashCode();
    }

    public static bool operator ==(WeightModifierHandle left, WeightModifierHandle right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(WeightModifierHandle left, WeightModifierHandle right)
    {
        return !left.Equals(right);
    }
}