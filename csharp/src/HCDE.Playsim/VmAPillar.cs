namespace HCDE.Playsim;

/// <summary><c>DPillar</c> and <c>EV_DoPillar</c> from <c>mapthinkers/a_pillar.cpp</c> (47–259).</summary>
public static class VmAPillar
{
    public const int PillarBuild = 0;
    public const int PillarOpen = 1;
    public const int SecfSilentmove = 1 << 0;
    public const int ChanFloor = 0;

    public sealed class PillarSector
    {
        public int Flags { get; set; }
        public int SeqType { get; set; } = -1;
        public string SeqName { get; set; } = "";
        public object? FloorData { get; set; }
        public object? CeilingData { get; set; }
        public Func<double> CenterFloor { get; init; } = () => 0;
        public Func<double> CenterCeiling { get; init; } = () => 0;
        public object Centerspot { get; init; } = new();
        public Func<object, double, double> FloorPointToDist { get; init; } = (_, _) => 0;
        public Func<object, double, double> CeilingPointToDist { get; init; } = (_, _) => 0;
        public Func<object, double> FloorZAtPoint { get; init; } = _ => 0;
        public Func<object, double> CeilingZAtPoint { get; init; } = _ => 0;
        public Func<bool> PlaneMovingFloor { get; init; } = () => false;
        public Func<bool> PlaneMovingCeiling { get; init; } = () => false;
        public Action<object?, object?> SetFloorAndCeilingData { get; init; } = (_, _) => { };
        public Func<object, object?> SetInterpolation { get; init; } = _ => null;
    }

    public sealed class PillarThinker
    {
        public PillarSector Sector { get; set; } = null!;
        public int Type { get; set; }
        public double FloorSpeed { get; set; }
        public double CeilingSpeed { get; set; }
        public double FloorTarget { get; set; }
        public double CeilingTarget { get; set; }
        public int Crush { get; set; }
        public bool Hexencrush { get; set; }
        public object? InterpFloor { get; set; }
        public object? InterpCeiling { get; set; }
        public Action DestroySelf { get; init; } = () => { };
        public Action<object?> DelRefFloor { get; init; } = _ => { };
        public Action<object?> DelRefCeiling { get; init; } = _ => { };
        public Action<PillarSector> StopSequence { get; init; } = _ => { };
        public Func<PillarSector, double, double, int, int, bool, VmADoorTickSound.MoveResult> MoveFloor { get; init; } =
            (_, _, _, _, _, _) => VmADoorTickSound.MoveResult.Ok;
        public Func<PillarSector, double, double, int, int, bool, VmADoorTickSound.MoveResult> MoveCeiling { get; init; } =
            (_, _, _, _, _, _) => VmADoorTickSound.MoveResult.Ok;
        public Func<PillarSector, double> GetFloorD { get; init; } = _ => 0;
        public Func<PillarSector, double> GetCeilingD { get; init; } = _ => 0;
    }

    public static void OnDestroy(PillarThinker pillar)
    {
        if (pillar.InterpCeiling != null)
        {
            pillar.DelRefCeiling(pillar.InterpCeiling);
            pillar.InterpCeiling = null;
        }

        if (pillar.InterpFloor != null)
        {
            pillar.DelRefFloor(pillar.InterpFloor);
            pillar.InterpFloor = null;
        }
    }

    public static void Tick(PillarThinker pillar)
    {
        var oldFloor = pillar.GetFloorD(pillar.Sector);
        var oldCeiling = pillar.GetCeilingD(pillar.Sector);
        VmADoorTickSound.MoveResult r;
        VmADoorTickSound.MoveResult s;
        if (pillar.Type == PillarBuild)
        {
            r = pillar.MoveFloor(pillar.Sector, pillar.FloorSpeed, pillar.FloorTarget, pillar.Crush, 1, pillar.Hexencrush);
            s = pillar.MoveCeiling(pillar.Sector, pillar.CeilingSpeed, pillar.CeilingTarget, pillar.Crush, -1, pillar.Hexencrush);
        }
        else
        {
            r = pillar.MoveFloor(pillar.Sector, pillar.FloorSpeed, pillar.FloorTarget, pillar.Crush, -1, pillar.Hexencrush);
            s = pillar.MoveCeiling(pillar.Sector, pillar.CeilingSpeed, pillar.CeilingTarget, pillar.Crush, 1, pillar.Hexencrush);
        }

        if (r == VmADoorTickSound.MoveResult.PastDest && s == VmADoorTickSound.MoveResult.PastDest)
        {
            pillar.StopSequence(pillar.Sector);
            pillar.DestroySelf();
        }
        else
        {
            if (r == VmADoorTickSound.MoveResult.Crushed)
                pillar.MoveFloor(pillar.Sector, pillar.FloorSpeed, oldFloor, -1, -1, pillar.Hexencrush);
            if (s == VmADoorTickSound.MoveResult.Crushed)
                pillar.MoveCeiling(pillar.Sector, pillar.CeilingSpeed, oldCeiling, -1, 1, pillar.Hexencrush);
        }
    }

    public static void Construct(
        PillarThinker pillar,
        PillarSector sector,
        int type,
        double speed,
        double floordist,
        double ceilingdist,
        int crush,
        bool hexencrush,
        Func<PillarSector, (object? spot, double height)> findLowestFloorSurrounding,
        Func<PillarSector, (object? spot, double height)> findHighestCeilingSurrounding,
        Action<PillarSector, int, string, int> startSequence)
    {
        pillar.Sector = sector;
        sector.SetFloorAndCeilingData(pillar, pillar);
        pillar.InterpFloor = sector.SetInterpolation(sector);
        pillar.InterpCeiling = sector.SetInterpolation(sector);
        pillar.Type = type;
        pillar.Crush = crush;
        pillar.Hexencrush = hexencrush;

        double newheight;
        if (type == PillarBuild)
        {
            if (floordist == 0)
            {
                newheight = (sector.CenterFloor() + sector.CenterCeiling()) / 2;
                pillar.FloorTarget = sector.FloorPointToDist(sector.Centerspot, newheight);
                pillar.CeilingTarget = sector.CeilingPointToDist(sector.Centerspot, newheight);
                floordist = newheight - sector.CenterFloor();
            }
            else
            {
                newheight = sector.CenterFloor() + floordist;
                pillar.FloorTarget = sector.FloorPointToDist(sector.Centerspot, newheight);
                pillar.CeilingTarget = sector.CeilingPointToDist(sector.Centerspot, newheight);
            }

            ceilingdist = sector.CenterCeiling() - newheight;
        }
        else
        {
            if (floordist == 0)
            {
                var (spot, low) = findLowestFloorSurrounding(sector);
                newheight = low;
                pillar.FloorTarget = sector.FloorPointToDist(spot!, newheight);
                floordist = sector.FloorZAtPoint(spot!) - newheight;
            }
            else
            {
                newheight = sector.CenterFloor() - floordist;
                pillar.FloorTarget = sector.FloorPointToDist(sector.Centerspot, newheight);
            }

            if (ceilingdist == 0)
            {
                var (spot, high) = findHighestCeilingSurrounding(sector);
                newheight = high;
                pillar.CeilingTarget = sector.CeilingPointToDist(spot!, newheight);
                ceilingdist = newheight - sector.CeilingZAtPoint(spot!);
            }
            else
            {
                newheight = sector.CenterCeiling() + ceilingdist;
                pillar.CeilingTarget = sector.CeilingPointToDist(sector.Centerspot, newheight);
            }
        }

        if (floordist > ceilingdist)
        {
            pillar.FloorSpeed = speed;
            pillar.CeilingSpeed = ceilingdist == 0 ? 0 : speed * ceilingdist / floordist;
        }
        else
        {
            pillar.CeilingSpeed = speed;
            pillar.FloorSpeed = floordist == 0 ? 0 : speed * floordist / ceilingdist;
        }

        if ((sector.Flags & SecfSilentmove) == 0)
        {
            if (sector.SeqType >= 0)
                startSequence(sector, ChanFloor, sector.SeqType.ToString(), 0);
            else if (!string.IsNullOrEmpty(sector.SeqName))
                startSequence(sector, ChanFloor, sector.SeqName, 0);
            else
                startSequence(sector, ChanFloor, "Floor", 0);
        }
    }

    public static bool EvDoPillar(
        int type,
        IEnumerable<int> sectorNums,
        Func<int, PillarSector?> resolveSector,
        double speed,
        double height,
        double height2,
        int crush,
        bool hexencrush,
        Action<PillarSector, int, double, double, double, int, bool> createThinker)
    {
        var rtn = false;
        foreach (var secnum in sectorNums)
        {
            var sec = resolveSector(secnum);
            if (sec == null)
                continue;
            if (sec.PlaneMovingFloor() || sec.PlaneMovingCeiling())
                continue;
            var flor = sec.CenterFloor();
            var ceil = sec.CenterCeiling();
            if (type == PillarBuild && flor == ceil)
                continue;
            if (type == PillarOpen && flor != ceil)
                continue;
            rtn = true;
            createThinker(sec, type, speed, height, height2, crush, hexencrush);
        }

        return rtn;
    }
}
