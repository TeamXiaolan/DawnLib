namespace Dawn;

public sealed class OutsideMapObjectBaseCurveSource : LevelListBaseCurveSource<SpawnableOutsideObjectWithRarity>
{
    public OutsideMapObjectBaseCurveSource(SpawnableOutsideObject spawnableOutsideObject) : base(level => level.spawnableOutsideObjects, entry => entry.spawnableObject == spawnableOutsideObject, entry => entry.randomAmount) { }
}