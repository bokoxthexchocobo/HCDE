namespace HCDE.Playsim;

/// <summary>
/// Ceiling construct, <c>CreateCeiling</c>, the action wrapper, and
/// <c>EV_DoCeiling</c> from <c>a_ceiling.cpp</c> (216-551). Chunk 1335.
/// Tick through line 215 and <c>ActivateInStasisCeiling</c> from line 552
/// stay chunk 797. <c>ECeiling</c> values follow <c>a_ceiling.h</c>.
/// </summary>
public sealed class VmACeilingConstruct1335
{
    public const int SecfFriction = 16;
    public const int SecfPush = 32;
    public const int SecfDmgTerrain = 128;
    public const int SecfEndGod = 256;
    public const int SecfEndLevel = 512;
    public const int SecfHazard = 1024;
    public const int SecfExit1 = 4096;
    public const int SecfExit2 = 8192;
    public const int SecfKillMonsters = 16384;
    public const int SecfDamageFlags = SecfEndGod | SecfEndLevel | SecfDmgTerrain | SecfHazard;
    public const int SpecialFlags = SecfDamageFlags | SecfFriction | SecfPush | SecfExit1 | SecfExit2 | SecfKillMonsters;
    public const int PlanefBlocked = 2;
    public const int CeilingMove = 0;
    public const int CrushDoom = 0;
    public const int CrushHexen = 1;
    public const int CrushSlowdown = 2;

    public enum ECeiling
    {
        LowerByValue = 0,
        RaiseByValue = 1,
        MoveToValue = 2,
        LowerToHighestFloor = 3,
        LowerInstant = 4,
        RaiseInstant = 5,
        CrushAndRaise = 6,
        LowerAndCrush = 7,
        Placeholder = 8,
        CrushRaiseAndStay = 9,
        RaiseToNearest = 10,
        LowerToLowest = 11,
        LowerToFloor = 12,
        RaiseToHighest = 13,
        LowerToHighest = 14,
        RaiseToLowest = 15,
        LowerToNearest = 16,
        RaiseToHighestFloor = 17,
        RaiseToFloor = 18,
        RaiseByTexture = 19,
        LowerByTexture = 20,
        GenChg0 = 21,
        GenChgT = 22,
        GenChg = 23,
    }

    public sealed class Vertex
    {
        public double X { get; set; }
        public double Y { get; set; }
    }

    public sealed class Spot
    {
        public Spot(Vertex current) => Current = current;

        public Vertex Current { get; set; }
    }

    public sealed class Special
    {
        public string DamageType { get; set; } = "";
        public int DamageAmount { get; set; }
        public int SpecialId { get; set; }
        public short DamageInterval { get; set; }
        public short LeakyDamage { get; set; }
        public int Flags { get; set; }
    }

    public sealed class Interpolation
    {
        public bool Cleared { get; private set; }
        public bool Force { get; private set; }

        public void DelRef(bool force)
        {
            Cleared = true;
            Force = force;
        }
    }

    public sealed class Line
    {
        public Vertex V1 { get; set; } = new();
        public Vertex V2 { get; set; } = new();
        public Sector? Front { get; set; }
        public Sector? Back { get; set; }
    }

    public sealed class Sector
    {
        public int SectorNum { get; set; }
        public double CeilingPlaneD { get; set; }
        public int CeilingPlaneFlags { get; set; }
        public int CeilingTexture { get; set; }
        public object? CeilingData { get; set; }
        public Special Special { get; set; } = new();
        public List<Line> Lines { get; } = new();
        public Func<Vertex, double>? ZatPoint { get; set; }
        public Func<Vertex, double, double>? PointToDist { get; set; }
        public Func<int, Interpolation?>? SetInterpolation { get; set; }

        public bool PlaneMovingCeiling()
        {
            return CeilingData != null || (CeilingPlaneFlags & PlanefBlocked) != 0;
        }
    }

    public sealed class Ceiling
    {
        public Ceiling(Sector sector) => Sector = sector;

        public Sector Sector { get; }
        public Interpolation? Interpolation { get; set; }
        public int Type { get; set; }
        public double BottomHeight { get; set; }
        public double TopHeight { get; set; }
        public double Speed { get; set; }
        public double Speed1 { get; set; }
        public double Speed2 { get; set; }
        public int Crush { get; set; }
        public int CrushMode { get; set; }
        public int Silent { get; set; }
        public int Direction { get; set; }
        public int OldDirection { get; set; }
        public int Tag { get; set; }
        public int Texture { get; set; }
        public Special NewSpecial { get; set; } = new();

        public void StopInterpolation(bool force)
        {
            if (Interpolation == null)
                return;
            Interpolation.DelRef(force);
            Interpolation = null;
        }
    }

    public sealed class Context
    {
        public Func<Sector, Spot, double>? FindHighestFloorPoint { get; set; }
        public Func<Sector, Spot, double>? FindHighestCeilingSurrounding { get; set; }
        public Func<Sector, Spot, double>? FindHighestFloorSurrounding { get; set; }
        public Func<Sector, Spot, double>? FindNextLowestCeiling { get; set; }
        public Func<Sector, Spot, double>? FindNextHighestCeiling { get; set; }
        public Func<Sector, Spot, double>? FindLowestCeilingSurrounding { get; set; }
        public Func<Sector, double>? FindShortestUpperAround { get; set; }
        public Func<Sector, double, Sector?>? FindModelFloorSector { get; set; }
        public Func<Sector, double, Sector?>? FindModelCeilingSector { get; set; }
        public Action<Ceiling>? PlayCeilingSound { get; set; }
        public Func<int, bool>? ActivateInStasisCeiling { get; set; }
        public IEnumerable<int>? TagSectors { get; set; }
        public IReadOnlyList<Sector> Sectors { get; set; } = Array.Empty<Sector>();
    }

    public static Ceiling Construct(Sector sec) => Link(sec);

    public static Ceiling Construct(Sector sec, double speed1, double speed2, int silent)
    {
        var ceiling = Link(sec);
        ceiling.Crush = -1;
        ceiling.CrushMode = CrushDoom;
        ceiling.Speed = speed1;
        ceiling.Speed1 = speed1;
        ceiling.Speed2 = speed2;
        ceiling.Silent = silent;
        ceiling.BottomHeight = 0;
        ceiling.TopHeight = 0;
        ceiling.Direction = 0;
        ceiling.Texture = 0;
        ceiling.Tag = 0;
        ceiling.OldDirection = 0;
        return ceiling;
    }

    public static bool CreateCeiling(
        Context ctx,
        Sector sec,
        int type,
        Line? line,
        int tag,
        double speed,
        double speed2,
        double height,
        int crush,
        int silent,
        int change,
        int crushMode)
    {
        if (sec.PlaneMovingCeiling())
            return false;

        var ceiling = Construct(sec, speed, speed2, silent & ~4);
        var spot = new Spot(sec.Lines[0].V1);
        double targheight = 0;

        switch ((ECeiling)type)
        {
            case ECeiling.CrushAndRaise:
            case ECeiling.CrushRaiseAndStay:
                ceiling.TopHeight = sec.CeilingPlaneD;
                goto case ECeiling.LowerAndCrush;
            case ECeiling.LowerAndCrush:
                targheight = Height(ctx.FindHighestFloorPoint, "FindHighestFloorPoint", sec, spot);
                targheight += height;
                ceiling.BottomHeight = ToDist(sec, spot.Current, targheight);
                ceiling.Direction = -1;
                break;

            case ECeiling.RaiseToHighest:
                targheight = Height(ctx.FindHighestCeilingSurrounding, "FindHighestCeilingSurrounding", sec, spot);
                ceiling.TopHeight = ToDist(sec, spot.Current, targheight);
                ceiling.Direction = 1;
                break;

            case ECeiling.LowerByValue:
                targheight = Zat(sec, spot.Current) - height;
                ceiling.BottomHeight = ToDist(sec, spot.Current, targheight);
                ceiling.Direction = -1;
                break;

            case ECeiling.RaiseByValue:
                targheight = Zat(sec, spot.Current) + height;
                ceiling.TopHeight = ToDist(sec, spot.Current, targheight);
                ceiling.Direction = 1;
                break;

            case ECeiling.MoveToValue:
            {
                var diff = height - Zat(sec, spot.Current);
                targheight = height;
                if (diff < 0)
                {
                    ceiling.BottomHeight = ToDist(sec, spot.Current, height);
                    ceiling.Direction = -1;
                }
                else
                {
                    ceiling.TopHeight = ToDist(sec, spot.Current, height);
                    ceiling.Direction = 1;
                }
                break;
            }

            case ECeiling.LowerToHighestFloor:
                targheight = Height(ctx.FindHighestFloorSurrounding, "FindHighestFloorSurrounding", sec, spot) + height;
                ceiling.BottomHeight = ToDist(sec, spot.Current, targheight);
                ceiling.Direction = -1;
                break;

            case ECeiling.RaiseToHighestFloor:
                targheight = Height(ctx.FindHighestFloorSurrounding, "FindHighestFloorSurrounding", sec, spot);
                ceiling.TopHeight = ToDist(sec, spot.Current, targheight);
                ceiling.Direction = 1;
                break;

            case ECeiling.LowerInstant:
                targheight = Zat(sec, spot.Current) - height;
                ceiling.BottomHeight = ToDist(sec, spot.Current, targheight);
                ceiling.Direction = -1;
                ceiling.Speed = height;
                break;

            case ECeiling.RaiseInstant:
                targheight = Zat(sec, spot.Current) + height;
                ceiling.TopHeight = ToDist(sec, spot.Current, targheight);
                ceiling.Direction = 1;
                ceiling.Speed = height;
                break;

            case ECeiling.LowerToNearest:
                targheight = Height(ctx.FindNextLowestCeiling, "FindNextLowestCeiling", sec, spot);
                ceiling.BottomHeight = ToDist(sec, spot.Current, targheight);
                ceiling.Direction = -1;
                break;

            case ECeiling.RaiseToNearest:
                targheight = Height(ctx.FindNextHighestCeiling, "FindNextHighestCeiling", sec, spot);
                ceiling.TopHeight = ToDist(sec, spot.Current, targheight);
                ceiling.Direction = 1;
                break;

            case ECeiling.LowerToLowest:
                targheight = Height(ctx.FindLowestCeilingSurrounding, "FindLowestCeilingSurrounding", sec, spot);
                ceiling.BottomHeight = ToDist(sec, spot.Current, targheight);
                ceiling.Direction = -1;
                break;

            case ECeiling.RaiseToLowest:
                targheight = Height(ctx.FindLowestCeilingSurrounding, "FindLowestCeilingSurrounding", sec, spot);
                ceiling.TopHeight = ToDist(sec, spot.Current, targheight);
                ceiling.Direction = 1;
                break;

            case ECeiling.LowerToFloor:
                targheight = Height(ctx.FindHighestFloorPoint, "FindHighestFloorPoint", sec, spot) + height;
                ceiling.BottomHeight = ToDist(sec, spot.Current, targheight);
                ceiling.Direction = -1;
                break;

            case ECeiling.RaiseToFloor:
                targheight = Height(ctx.FindHighestFloorPoint, "FindHighestFloorPoint", sec, spot) + height;
                ceiling.TopHeight = ToDist(sec, spot.Current, targheight);
                ceiling.Direction = 1;
                break;

            case ECeiling.LowerToHighest:
                targheight = Height(ctx.FindHighestCeilingSurrounding, "FindHighestCeilingSurrounding", sec, spot);
                ceiling.BottomHeight = ToDist(sec, spot.Current, targheight);
                ceiling.Direction = -1;
                break;

            case ECeiling.LowerByTexture:
                targheight = Zat(sec, spot.Current) - Shortest(ctx, sec);
                ceiling.BottomHeight = ToDist(sec, spot.Current, targheight);
                ceiling.Direction = -1;
                break;

            case ECeiling.RaiseByTexture:
                targheight = Zat(sec, spot.Current) + Shortest(ctx, sec);
                ceiling.TopHeight = ToDist(sec, spot.Current, targheight);
                ceiling.Direction = 1;
                break;
        }

        ceiling.Tag = tag;
        ceiling.Type = type;
        ceiling.Crush = crush;
        ceiling.CrushMode = crushMode;

        double movedist;
        if (ceiling.Direction < 0)
            movedist = sec.CeilingPlaneD - ceiling.BottomHeight;
        else
            movedist = ceiling.TopHeight - sec.CeilingPlaneD;

        if (ceiling.Speed >= movedist)
        {
            ceiling.StopInterpolation(true);
            if ((silent & 4) != 0)
                ceiling.Silent = 2;
        }

        if ((change & 3) != 0)
        {
            if ((change & 4) != 0)
            {
                var model = type == (int)ECeiling.RaiseToFloor || type == (int)ECeiling.LowerToFloor
                    ? Model(ctx.FindModelFloorSector, "FindModelFloorSector", sec, targheight)
                    : Model(ctx.FindModelCeilingSector, "FindModelCeilingSector", sec, targheight);
                if (model != null)
                {
                    ceiling.Texture = model.CeilingTexture;
                    ApplyNumericChange(ceiling, sec, change);
                }
            }
            else if (line != null)
            {
                ceiling.Texture = line.Front!.CeilingTexture;
                ApplyTriggerChange(ceiling, line.Front, change);
            }
        }

        ctx.PlayCeilingSound?.Invoke(ceiling);
        return ceiling.Sector.CeilingData == ceiling;
    }

    public static bool CallCreateCeiling(
        Context ctx,
        Sector? sec,
        int type,
        Line? line,
        double speed,
        double speed2,
        double height,
        int crush,
        int silent,
        int change,
        int crushMode)
    {
        if (sec == null)
            throw new ArgumentNullException(nameof(sec));
        return CreateCeiling(ctx, sec, type, line, 0, speed, speed2, height, crush, silent, change, crushMode);
    }

    public static bool EvDoCeiling(
        Context ctx,
        int type,
        Line? line,
        int tag,
        double speed,
        double speed2,
        double height,
        int crush,
        int silent,
        int change,
        int crushMode)
    {
        var rtn = false;
        if (tag == 0)
        {
            if (line == null || line.Back == null)
                return false;
            var sec = line.Back;
            tag ^= sec.SectorNum | 0x1000000;
            Activate(ctx, tag);
            return CreateCeiling(ctx, sec, type, line, tag, speed, speed2, height, crush, silent, change, crushMode);
        }

        if (type == (int)ECeiling.CrushAndRaise)
            rtn |= Activate(ctx, tag);

        foreach (var secnum in ctx.TagSectors ?? Array.Empty<int>())
        {
            if (secnum < 0)
                break;
            rtn |= CreateCeiling(
                ctx,
                ctx.Sectors[secnum],
                type,
                line,
                tag,
                speed,
                speed2,
                height,
                crush,
                silent,
                change,
                crushMode);
        }

        return rtn;
    }

    static Ceiling Link(Sector sec)
    {
        var ceiling = new Ceiling(sec);
        sec.CeilingData = ceiling;
        ceiling.Interpolation = sec.SetInterpolation?.Invoke(CeilingMove);
        return ceiling;
    }

    static Special CopySpecial(Sector sector)
    {
        var src = sector.Special;
        return new Special
        {
            DamageType = src.DamageType,
            DamageAmount = src.DamageAmount,
            SpecialId = src.SpecialId,
            DamageInterval = src.DamageInterval,
            LeakyDamage = src.LeakyDamage,
            Flags = src.Flags & SpecialFlags,
        };
    }

    static void ApplyNumericChange(Ceiling ceiling, Sector sec, int change)
    {
        switch (change & 3)
        {
            case 1:
                ceiling.NewSpecial = new Special();
                ceiling.Type = (int)ECeiling.GenChg0;
                break;
            case 2:
                ceiling.NewSpecial = CopySpecial(sec);
                ceiling.Type = (int)ECeiling.GenChgT;
                break;
            case 3:
                ceiling.Type = (int)ECeiling.GenChg;
                break;
        }
    }

    static void ApplyTriggerChange(Ceiling ceiling, Sector front, int change)
    {
        switch (change & 3)
        {
            case 1:
                ceiling.NewSpecial = new Special();
                ceiling.Type = (int)ECeiling.GenChg0;
                break;
            case 2:
                ceiling.NewSpecial = CopySpecial(front);
                ceiling.Type = (int)ECeiling.GenChgT;
                break;
            case 3:
                ceiling.Type = (int)ECeiling.GenChg;
                break;
        }
    }

    static bool Activate(Context ctx, int tag)
    {
        if (ctx.ActivateInStasisCeiling == null)
            throw new InvalidOperationException("ActivateInStasisCeiling");
        return ctx.ActivateInStasisCeiling(tag);
    }

    static double Height(Func<Sector, Spot, double>? fn, string name, Sector sec, Spot spot)
    {
        if (fn == null)
            throw new InvalidOperationException(name);
        return fn(sec, spot);
    }

    static double Shortest(Context ctx, Sector sec)
    {
        if (ctx.FindShortestUpperAround == null)
            throw new InvalidOperationException("FindShortestUpperAround");
        return ctx.FindShortestUpperAround(sec);
    }

    static Sector? Model(Func<Sector, double, Sector?>? fn, string name, Sector sec, double targheight)
    {
        if (fn == null)
            throw new InvalidOperationException(name);
        return fn(sec, targheight);
    }

    static double Zat(Sector sec, Vertex spot)
    {
        if (sec.ZatPoint == null)
            throw new InvalidOperationException("ZatPoint");
        return sec.ZatPoint(spot);
    }

    static double ToDist(Sector sec, Vertex spot, double z)
    {
        if (sec.PointToDist == null)
            throw new InvalidOperationException("PointToDist");
        return sec.PointToDist(spot, z);
    }
}
