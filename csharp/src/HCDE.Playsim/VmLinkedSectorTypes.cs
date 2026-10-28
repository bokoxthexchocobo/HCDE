namespace HCDE.Playsim;

/// <summary>Link flags from <c>p_linkedsectors.cpp</c>.</summary>
public static class VmLinkedSectorLinkType
{
    public const int None = 0;
    public const int Floor = 1;
    public const int Ceiling = 2;
    public const int Both = 3;
    public const int FloorMirrorFlag = 4;
    public const int CeilingMirrorFlag = 8;
    public const int FloorMirror = 5;
    public const int CeilingMirror = 10;
    public const int BothMirror = 15;
    public const int FloorBits = 5;
    public const int CeilingBits = 10;
    public const int FlagMask = 15;
}

public sealed class VmLinkedSectorEntry
{
    public int SectorIndex { get; init; }
    public int Type { get; init; }
}

public sealed class VmLinkedSectorPlane
{
    public List<VmLinkedSectorEntry> Sectors { get; } = [];
}
