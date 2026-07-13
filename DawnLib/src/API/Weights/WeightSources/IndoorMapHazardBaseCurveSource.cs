namespace Dawn;

public sealed class IndoorMapHazardBaseCurveSource : LevelListBaseCurveSource<IndoorMapHazard>
{
    public IndoorMapHazardBaseCurveSource(IndoorMapHazardType hazardType) : base(level => level.indoorMapHazards, entry => entry.hazardType == hazardType, entry => entry.numberToSpawn) { }
}