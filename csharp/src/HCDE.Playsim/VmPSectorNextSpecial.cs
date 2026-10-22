namespace HCDE.Playsim;

/// <summary>
/// Special-sector lookup and danger heuristics from <c>p_sectors.cpp</c> (65–162).
/// </summary>
public static class VmPSectorNextSpecial
{
    public const int FfExists = 0x1;
    public const int FfSolid = 0x2;

    public sealed class SpecialLine
    {
        public SpecialSector? FrontSector { get; set; }
        public SpecialSector? BackSector { get; set; }
    }

    public sealed class SpecialSector
    {
        public List<SpecialLine> Lines { get; } = new();
        public int Special { get; set; }
        public int ValidCount { get; set; }
    }

    public readonly record struct SectorVec3(double X, double Y, double Z);

    public sealed class SecActTarget
    {
        public int Special { get; set; }
        public int[] Args { get; } = new int[5];
        public SecActTarget? Tracer { get; set; }
    }

    public sealed class DangerSectorModel
    {
        public int DamageAmount { get; set; }
        public int LastDamage { get; set; } = -1;
        public int MapTime { get; set; }
        public SecActTarget? SecActTarget { get; set; }
        public Func<int, bool>? SectorHasTag { get; set; }
        public object? CeilingData { get; set; }
        public object? FloorData { get; set; }
        public Func<object?, int>? GetCeilingCrush { get; set; }
        public Func<object?, int>? GetFloorCrush { get; set; }
        public List<FFloorRover>? FFloors { get; set; }
    }

    public sealed class FFloorRover
    {
        public int Flags { get; set; }
        public DangerSectorModel Model { get; } = new();
        public VmPSectorSurroundHeights.Plane Top { get; } = new();
        public VmPSectorSurroundHeights.Plane Bottom { get; } = new();
    }

    public delegate SpecialSector? GetNextSpecialSector(SpecialLine line, SpecialSector from);

    public static SpecialSector? GetNextSector(SpecialLine line, SpecialSector from)
    {
        if (line.FrontSector == from)
            return line.BackSector;
        if (line.BackSector == from)
            return line.FrontSector;
        return null;
    }

    public static SpecialSector? NextSpecialSectorVc(SpecialSector sec, int type, int validCount, GetNextSpecialSector? getNext = null)
    {
        getNext ??= GetNextSector;
        foreach (var ln in sec.Lines)
        {
            if (getNext(ln, sec) is { } tsec
                && tsec.ValidCount != validCount
                && tsec.Special == type)
            {
                return tsec;
            }
        }

        return null;
    }

    public static SpecialSector? NextSpecialSector(
        SpecialSector sec,
        int type,
        SpecialSector? nogood,
        GetNextSpecialSector? getNext = null)
    {
        getNext ??= GetNextSector;
        foreach (var ln in sec.Lines)
        {
            if (getNext(ln, sec) is { } tsec
                && tsec != nogood
                && tsec.Special == type)
            {
                return tsec;
            }
        }

        return null;
    }

    public static SpecialSector? NextSpecialSectorAction(SpecialSector self, int type, SpecialSector? nogood) =>
        NextSpecialSector(self, type, nogood);

    public static bool IsDamaging(DangerSectorModel sec, int moTid, int gameTicRate = 35)
    {
        var damageTime = gameTicRate * 5;
        if (sec.DamageAmount > 0 || (sec.LastDamage >= 0 && sec.MapTime - sec.LastDamage <= damageTime))
            return true;

        for (var secAct = sec.SecActTarget; secAct is not null; secAct = secAct.Tracer)
        {
            if ((secAct.Special == 73 && secAct.Args[0] >= 0)
                || (secAct.Special == 119 && secAct.Args[1] > 0 && (secAct.Args[0] == 0 || secAct.Args[1] == moTid))
                || (secAct.Special == 214 && secAct.Args[1] > 0
                    && (secAct.Args[0] == 0 || sec.SectorHasTag?.Invoke(secAct.Args[0]) == true)))
            {
                return true;
            }
        }

        return false;
    }

    public static bool IsCrushing(DangerSectorModel sec)
    {
        if ((sec.GetCeilingCrush?.Invoke(sec.CeilingData) ?? 0) > 0)
            return true;

        return (sec.GetFloorCrush?.Invoke(sec.FloorData) ?? 0) > 0;
    }

    public static bool IsDangerous(
        DangerSectorModel sec,
        SectorVec3 pos,
        double height,
        int moTid,
        IReadOnlyList<FFloorRover>? ffloors = null,
        int gameTicRate = 35)
    {
        if (IsDamaging(sec, moTid, gameTicRate) || IsCrushing(sec))
            return true;

        var rovers = ffloors ?? sec.FFloors;
        if (rovers is null)
            return false;

        foreach (var rover in rovers)
        {
            if ((rover.Flags & FfExists) == 0)
                continue;

            if (IsDamaging(rover.Model, moTid, gameTicRate)
                && pos.Z <= rover.Top.ZAtPoint(pos.X, pos.Y)
                && pos.Z + height >= rover.Bottom.ZAtPoint(pos.X, pos.Y))
            {
                return true;
            }

            if ((rover.Flags & FfSolid) != 0 && IsCrushing(rover.Model))
                return true;
        }

        return false;
    }
}
