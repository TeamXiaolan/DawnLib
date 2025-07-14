using System;

namespace CodeRebirthLib.ConfigManagement.Converters;
public abstract class TOMLConverter
{
    public abstract Type ConvertingType { get; }
    public abstract string Serialize(object raw);
    public abstract object Deserialize(string value);

    public virtual bool IsEnabled() => true;
}

public abstract class TOMLConverter<T> : TOMLConverter
{
    public override Type ConvertingType => typeof(T);

    public override string Serialize(object raw)
    {
        return ConvertToString((T)raw);
    }
    public override object Deserialize(string value)
    {
        return ConvertToObject(value);
    }

    protected abstract string ConvertToString(T value);
    protected abstract T ConvertToObject(string value);
}