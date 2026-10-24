namespace HCDE.Playsim;

/// <summary>Five whole functions from <c>a_floor.cpp</c> (114–225, 233–251, 259–262, 992–995, 1010–1095).</summary>
public static class VmAFloorTickElevator
{
    public const int SecfSilentmove = 1 << 14;
    public const int DonutRaise = 22;
    public const int BuildStair = 23;
    public const int WaitStair = 24;
    public const int ResetStair = 25;
    public const int GenFloorChg0 = 26;
    public const int GenFloorChgT = 27;
    public const int GenFloorChg = 28;
    public const int FloorLowerAndChange = 15;
    public const int ElevateUp = 0;
    public const int ElevateDown = 1;
    public const int ElevateCurrent = 2;
    public const int ElevateRaise = 3;
    public const int ElevateLower = 4;
    public const int StairlockBuilding = -2;
    public const int StairlockDone = -1;

    public sealed class VertexSpot
    {
        public double X { get; set; }
        public double Y { get; set; }
    }

    public sealed class FloorSector
    {
        public int SectorNum { get; set; }
        public uint Flags { get; set; }
        public int SeqType { get; set; } = -1;
        public string SeqName { get; set; } = "";
        public double FloorPlaneD { get; set; }
        public int Stairlock { get; set; }
        public int Prevsec { get; set; } = -1;
        public int Nextsec { get; set; } = -1;
        public object? FloorData { get; set; }
        public object? CeilingData { get; set; }
        public double CenterFloor { get; set; }
        public double CenterCeiling { get; set; }
        public VertexSpot Centerspot { get; } = new();
        public string FloorTexture { get; set; } = "";
        public SectorSpecial Special { get; set; } = new();
        public Func<double, double, double>? FloorZAtPoint { get; set; }
        public Func<double, double, double>? CeilingZAtPoint { get; set; }
        public Func<double, double, double, double>? PointToDist { get; set; }
        public Func<double, double, double, double>? CeilingPointToDist { get; set; }
        public Action<SectorSpecial>? SetSpecial { get; set; }
        public Action<string>? SetFloorTexture { get; set; }
    }

    public sealed class SectorSpecial
    {
        public int Tag { get; set; }
    }

    public sealed class FloorThinker
    {
        public required FloorSector Sector { get; init; }
        public int Type { get; set; }
        public int Direction { get; set; }
        public double Speed { get; set; }
        public double FloorDestDist { get; set; }
        public int Crush { get; set; }
        public bool Hexencrush { get; set; }
        public bool Instant { get; set; }
        public int ResetCount { get; set; }
        public double OrgDist { get; set; }
        public int Delay { get; set; }
        public int PauseTime { get; set; }
        public int StepTime { get; set; }
        public int PerStepTime { get; set; }
        public string Texture { get; set; } = "";
        public SectorSpecial NewSpecial { get; set; } = new();
        public bool Destroyed { get; set; }
    }

    public sealed class ElevatorThinker
    {
        public required FloorSector Sector { get; init; }
        public int Type { get; set; }
        public int Direction { get; set; }
        public double Speed { get; set; }
        public double FloorDestDist { get; set; }
        public double CeilingDestDist { get; set; }
    }

    public sealed class FloorLine
    {
        public VertexSpot V1 { get; } = new();
        public FloorSector? FrontSector { get; set; }
    }

    public sealed class SoundContext
    {
        public Action<FloorSector, int, int, int, int>? StartSequenceType { get; init; }
        public Action<FloorSector, int, string, int>? StartSequenceName { get; init; }
        public Action<FloorSector, int, string, int>? StartSequenceSound { get; init; }
    }

    public sealed class FloorTickContext
    {
        public SoundContext Sound { get; init; } = new();
        public Func<FloorSector, double, double, int, int, bool, bool, VmADoorTickSound.MoveResult>? MoveFloor { get; init; }
        public Action<FloorSector>? StopSequence { get; init; }
        public Action<FloorThinker>? StopInterpolation { get; init; }
        public Action<FloorThinker>? Destroy { get; init; }
        public Func<int, FloorSector?>? SectorByIndex { get; init; }
    }

    public sealed class EvDoElevatorContext
    {
        public SoundContext Sound { get; init; } = new();
        public Func<int, IEnumerable<int>>? SectorTagIterator { get; init; }
        public Func<int, FloorSector?>? SectorByIndex { get; init; }
        public Func<FloorSector, bool>? PlaneMovingFloor { get; init; }
        public Func<FloorSector, ElevatorThinker>? CreateElevator { get; init; }
        public Func<FloorSector, VertexSpot?, double>? FindNextLowestFloor { get; init; }
        public Func<FloorSector, VertexSpot?, double>? FindNextHighestFloor { get; init; }
    }

    public static void PlayFloorSound(FloorSector sec, SoundContext ctx)
    {
        if ((sec.Flags & SecfSilentmove) != 0)
            return;
        if (sec.SeqType >= 0)
            ctx.StartSequenceType?.Invoke(sec, 0, sec.SeqType, 3, 0);
        else if (sec.SeqName.Length > 0)
            ctx.StartSequenceName?.Invoke(sec, 0, sec.SeqName, 0);
        else
            ctx.StartSequenceSound?.Invoke(sec, 0, "Floor", 0);
    }

    public static void StartFloorSound(FloorThinker floor, SoundContext ctx) =>
        PlayFloorSound(floor.Sector, ctx);

    public static void StartElevatorFloorSound(ElevatorThinker elevator, SoundContext ctx) =>
        PlayFloorSound(elevator.Sector, ctx);

    public static void SetFloorChangeType(FloorThinker floor, FloorSector sec, int change)
    {
        floor.Texture = sec.FloorTexture;
        switch (change & 3)
        {
            case 1:
                floor.NewSpecial = new SectorSpecial();
                floor.Type = GenFloorChg0;
                break;
            case 2:
                floor.Type = GenFloorChg;
                break;
            case 3:
                floor.NewSpecial = sec.Special;
                floor.Type = GenFloorChgT;
                break;
        }
    }

    public static void Tick(FloorThinker floor, FloorTickContext ctx)
    {
        var sec = floor.Sector;
        if (floor.Type == BuildStair || floor.Type == WaitStair)
        {
            if (floor.ResetCount != 0)
            {
                if (--floor.ResetCount == 0)
                {
                    floor.Type = ResetStair;
                    floor.Direction = floor.Direction > 0 ? -1 : 1;
                    floor.FloorDestDist = floor.OrgDist;
                }
            }
            if (floor.PauseTime != 0)
            {
                floor.PauseTime--;
                return;
            }
            if (floor.StepTime != 0)
            {
                if (--floor.StepTime == 0)
                {
                    floor.PauseTime = floor.Delay;
                    floor.StepTime = floor.PerStepTime;
                }
            }
        }

        if (floor.Type == WaitStair)
            return;

        var res = ctx.MoveFloor?.Invoke(sec, floor.Speed, floor.FloorDestDist, floor.Crush, floor.Direction, floor.Hexencrush, floor.Instant)
            ?? VmADoorTickSound.MoveResult.Ok;

        if (res != VmADoorTickSound.MoveResult.PastDest)
            return;

        ctx.StopSequence?.Invoke(sec);

        if (floor.Type == BuildStair)
            floor.Type = WaitStair;

        if (floor.Type != WaitStair || floor.ResetCount == 0)
        {
            if (floor.Direction == 1)
            {
                switch (floor.Type)
                {
                    case DonutRaise:
                    case GenFloorChgT:
                    case GenFloorChg0:
                        sec.SetSpecial?.Invoke(floor.NewSpecial);
                        goto case GenFloorChg;
                    case GenFloorChg:
                        sec.SetFloorTexture?.Invoke(floor.Texture);
                        break;
                }
            }
            else if (floor.Direction == -1)
            {
                switch (floor.Type)
                {
                    case FloorLowerAndChange:
                    case GenFloorChgT:
                    case GenFloorChg0:
                        sec.SetSpecial?.Invoke(floor.NewSpecial);
                        goto case GenFloorChg;
                    case GenFloorChg:
                        sec.SetFloorTexture?.Invoke(floor.Texture);
                        break;
                }
            }

            sec.FloorData = null;
            ctx.StopInterpolation?.Invoke(floor);

            if (sec.Stairlock == StairlockBuilding)
            {
                var chain = sec;
                chain.Stairlock = StairlockDone;
                while (chain.Prevsec != -1)
                {
                    var prev = ctx.SectorByIndex?.Invoke(chain.Prevsec);
                    if (prev == null || prev.Stairlock == StairlockBuilding)
                        break;
                    chain = prev;
                }
                if (chain.Prevsec == -1)
                {
                    chain = sec;
                    while (chain.Nextsec != -1)
                    {
                        var next = ctx.SectorByIndex?.Invoke(chain.Nextsec);
                        if (next == null || next.Stairlock == StairlockBuilding)
                            break;
                        chain = next;
                    }
                    if (chain.Nextsec == -1)
                    {
                        chain = sec;
                        while (chain.Prevsec != -1)
                        {
                            var prev = ctx.SectorByIndex?.Invoke(chain.Prevsec);
                            if (prev != null)
                            {
                                prev.Stairlock = 0;
                                chain = prev;
                            }
                            else break;
                        }
                        sec.Stairlock = 0;
                    }
                }
            }

            floor.Destroyed = true;
            ctx.Destroy?.Invoke(floor);
        }
    }

    public static bool EvDoElevator(
        EvDoElevatorContext ctx,
        FloorLine? line,
        int elevType,
        double speed,
        double height,
        int tag)
    {
        if (line == null && elevType == ElevateCurrent)
            return false;

        var rtn = false;
        FloorSector? lastSec = null;
        foreach (var secnum in ctx.SectorTagIterator?.Invoke(tag) ?? Array.Empty<int>())
        {
            var sec = ctx.SectorByIndex?.Invoke(secnum);
            if (sec == null)
                continue;
            if (ctx.PlaneMovingFloor?.Invoke(sec) == true || sec.CeilingData != null)
                continue;

            rtn = true;
            lastSec = sec;
            var elevator = ctx.CreateElevator?.Invoke(sec);
            if (elevator == null)
                continue;
            elevator.Type = elevType;
            elevator.Speed = speed;
            StartElevatorFloorSound(elevator, ctx.Sound);

            var floorheight = sec.CenterFloor;
            var ceilingheight = sec.CenterCeiling;
            var spot = new VertexSpot();
            double newheight;

            switch (elevType)
            {
                case ElevateDown:
                    elevator.Direction = -1;
                    newheight = ctx.FindNextLowestFloor?.Invoke(sec, spot) ?? 0;
                    elevator.FloorDestDist = sec.PointToDist?.Invoke(spot.X, spot.Y, newheight) ?? 0;
                    newheight += (sec.CeilingZAtPoint?.Invoke(spot.X, spot.Y) ?? 0)
                        - (sec.FloorZAtPoint?.Invoke(spot.X, spot.Y) ?? 0);
                    elevator.CeilingDestDist = CeilingDist(sec, spot.X, spot.Y, newheight);
                    break;
                case ElevateUp:
                    elevator.Direction = 1;
                    newheight = ctx.FindNextHighestFloor?.Invoke(sec, spot) ?? 0;
                    elevator.FloorDestDist = sec.PointToDist?.Invoke(spot.X, spot.Y, newheight) ?? 0;
                    newheight += (sec.CeilingZAtPoint?.Invoke(spot.X, spot.Y) ?? 0)
                        - (sec.FloorZAtPoint?.Invoke(spot.X, spot.Y) ?? 0);
                    elevator.CeilingDestDist = CeilingDist(sec, spot.X, spot.Y, newheight);
                    break;
                case ElevateCurrent:
                    if (line?.FrontSector == null)
                        break;
                    newheight = line.FrontSector.FloorZAtPoint?.Invoke(line.V1.X, line.V1.Y) ?? 0;
                    elevator.FloorDestDist = sec.PointToDist?.Invoke(line.V1.X, line.V1.Y, newheight) ?? 0;
                    newheight += (sec.CeilingZAtPoint?.Invoke(line.V1.X, line.V1.Y) ?? 0)
                        - (sec.FloorZAtPoint?.Invoke(line.V1.X, line.V1.Y) ?? 0);
                    elevator.CeilingDestDist = CeilingDist(sec, line.V1.X, line.V1.Y, newheight);
                    elevator.Direction = elevator.FloorDestDist > sec.FloorPlaneD ? -1 : 1;
                    break;
                case ElevateRaise:
                    elevator.Direction = 1;
                    elevator.FloorDestDist = sec.PointToDist?.Invoke(sec.Centerspot.X, sec.Centerspot.Y, floorheight + height) ?? 0;
                    elevator.CeilingDestDist = CeilingDist(sec, sec.Centerspot.X, sec.Centerspot.Y, ceilingheight + height);
                    break;
                case ElevateLower:
                    elevator.Direction = -1;
                    elevator.FloorDestDist = sec.PointToDist?.Invoke(sec.Centerspot.X, sec.Centerspot.Y, floorheight - height) ?? 0;
                    elevator.CeilingDestDist = CeilingDist(sec, sec.Centerspot.X, sec.Centerspot.Y, ceilingheight - height);
                    break;
            }
        }

        return rtn;
    }

    static double CeilingDist(FloorSector sec, double x, double y, double z) =>
        sec.CeilingPointToDist?.Invoke(x, y, z) ?? sec.PointToDist?.Invoke(x, y, z) ?? 0;
}
