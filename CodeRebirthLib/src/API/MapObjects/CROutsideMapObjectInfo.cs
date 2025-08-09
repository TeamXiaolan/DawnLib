namespace CodeRebirthLib;

public sealed class CROutsideMapObjectInfo
{
    public CRMapObjectInfo ParentInfo { get; internal set; }

    internal CROutsideMapObjectInfo(bool alignWithTerrain)
    {
        AlignWithTerrain = alignWithTerrain;
    }

    public bool AlignWithTerrain { get; }
}