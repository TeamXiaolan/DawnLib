
using Dawn.Utils;

namespace Dawn.Internal;
class BoundedRangeConverter : TOMLConverter<BoundedRange>
{
    protected override string ConvertToString(BoundedRange range)
    {
        return $"{range.Min},{range.Max}";
    }

    protected override BoundedRange ConvertToObject(string value)
    {
        string[] parts = value.Split(",");
        float min = 0, max = 0;
        if (parts.Length != 2 || !float.TryParse(parts[0], out min) || !float.TryParse(parts[1], out max))
        {
            DawnPlugin.Logger.LogError($"Failed to parse BoundedRange value: {value}");
        }
        return new BoundedRange(min, max);
    }
}