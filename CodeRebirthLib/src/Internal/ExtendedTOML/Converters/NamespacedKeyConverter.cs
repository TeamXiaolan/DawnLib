namespace CodeRebirthLib.Internal;
class NamespacedKeyConverter : TOMLConverter<NamespacedKey>
{

    protected override string ConvertToString(NamespacedKey value)
    {
        return value.ToString();
    }
    protected override NamespacedKey ConvertToObject(string value)
    {
        return NamespacedKey.Parse(value);
    }
}