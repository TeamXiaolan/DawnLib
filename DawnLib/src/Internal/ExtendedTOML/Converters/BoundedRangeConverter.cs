
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
        return new BoundedRange(int.Parse(parts[0]), int.Parse(parts[1]));
    }
}