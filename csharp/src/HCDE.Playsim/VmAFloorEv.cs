namespace HCDE.Playsim;

/// <summary>Shared floor-EV context types; donut/change on chunk **920** <see cref="VmAFloorEvDonutChange920Slice"/>; line floor/stair on **919**.</summary>
public static class VmAFloorEv
{
    public const int BuildUp = 0;
    public const int BuildDown = 1;
    public const int StairUseSpecials = 1;
    public const int StairSync = 2;
    public const int StairCrush = 4;
    public const int CompatfStairindex = 1;
    public const int Compatf2Floormove = 1;
    public const int TrigChangeOnly = 0;
    public const int NumChangeOnly = 1;
    public const int StairsSpecial1 = 26;
    public const int StairsSpecial2 = 27;

    public sealed class EvLine
    {
        public StairSector? FrontSector { get; set; }
    }

    public sealed class StairLine
    {
        public bool TwoSided { get; set; }
        public StairSector? FrontSector { get; set; }
        public StairSector? BackSector { get; set; }
    }

    public sealed class StairSector
    {
        public int SectorNum { get; set; }
        public int Stairlock { get; set; }
        public int Prevsec { get; set; } = -1;
        public int Nextsec { get; set; } = -1;
        public int Validcount { get; set; }
        public string FloorTexture { get; set; } = "";
        public double FloorPlaneD { get; set; }
        public VmAFloorTickElevator.VertexSpot Centerspot { get; } = new();
        public int Special { get; set; }
        public List<StairLine> Lines { get; } = new();
        public object? FloorData { get; set; }
        public Func<double, double, double, double>? PointToDist { get; set; }
        public double CenterFloor { get; set; }
    }

    public sealed class FloorEvContext
    {
        public int CompatFlags { get; init; }
        public int CompatFlags2 { get; init; }
        public Func<int, EvLine?, IEnumerable<int>>? SectorTagIterator { get; init; }
        public Func<int, StairSector?>? SectorByIndex { get; init; }
        public Func<int, int, IEnumerable<int>>? SectorTagIteratorCompat { get; init; }
        public VmAFloorCreate.CreateFloorContext? CreateFloor { get; init; }
        public Func<StairSector, VmAFloorCreate.FloorSector?>? ToCreateSector { get; init; }
        public Func<StairSector, bool>? PlaneMovingFloor { get; init; }
        public Func<StairSector, VmAFloorTickElevator.FloorThinker>? CreateStairThinker { get; init; }
        public Action<VmAFloorTickElevator.FloorThinker>? StartFloorSound { get; init; }
        public Func<StairSector, int, StairSector?>? NextSpecialSector { get; init; }
        public Func<StairSector, int>? BumpValidcount { get; init; }
        public Func<StairSector, VmAFloorTickElevator.VertexSpot, double>? FindHighestFloorPoint { get; init; }
        public Func<StairSector, StairLine, StairSector, StairSector?>? GetNextSector { get; init; }
        public Func<StairSector, bool>? IsFloorThinker { get; init; }
        public Func<StairSector, int>? GetFloorThinkerType { get; init; }
        public Action<StairSector>? StopSequence { get; init; }
        public Action<StairSector>? DestroyFloorData { get; init; }
        public Action<StairSector, string>? SetFloorTexture { get; init; }
        public Action<StairSector, StairSector>? TransferSpecial { get; init; }
        public Func<StairSector, double, StairSector?>? FindModelFloorSector { get; init; }
    }

}
