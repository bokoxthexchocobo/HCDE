namespace HCDE.Playsim;

/// <summary><c>FLevelLocals::CreateCeiling</c> from <c>a_ceiling.cpp</c> (250–479).</summary>
public static class VmACeilingCreate
{
    public const int SecfSilentmove = 1 << 14;
    public const int CeilCrushAndRaise = 8;
    public const int CeilLowerAndCrush = 9;
    public const int CeilCrushRaiseAndStay = 11;
    public const int CeilRaiseToHighest = 19;
    public const int CeilLowerByValue = 0;
    public const int CeilRaiseByValue = 1;
    public const int CeilMoveToValue = 2;
    public const int CeilLowerToHighestFloor = 3;
    public const int CeilLowerInstant = 4;
    public const int CeilRaiseInstant = 5;
    public const int CeilRaiseToNearest = 12;
    public const int CeilLowerToLowest = 13;
    public const int CeilLowerToFloor = 14;
    public const int CeilLowerToNearest = 15;
    public const int CeilRaiseToLowest = 18;
    public const int CeilRaiseToHighestFloor = 20;
    public const int CeilRaiseToFloor = 21;
    public const int CeilLowerToHighest = 22;
    public const int CeilLowerByTexture = 23;
    public const int CeilRaiseByTexture = 24;
    public const int GenCeilingChg0 = 25;
    public const int GenCeilingChgT = 26;
    public const int GenCeilingChg = 27;
    public const int CrushDoom = 0;
    public const int CrushHexen = 1;
    public const int CrushSlowdown = 2;

    public sealed class VertexSpot
    {
        public double X { get; set; }
        public double Y { get; set; }
    }

    public sealed class CeilingSector
    {
        public int SectorNum { get; set; }
        public uint Flags { get; set; }
        public int SeqType { get; set; } = -1;
        public string SeqName { get; set; } = "";
        public double CeilingPlaneD { get; set; }
        public Func<double, double, double>? CeilingZAtPoint { get; set; }
        public Func<double, double, double, double>? PointToDist { get; set; }
        public List<CeilingLine> Lines { get; } = new();
        public string CeilingTexture { get; set; } = "";
        public SectorSpecial Special { get; set; } = new();
        public object? CeilingData { get; set; }
    }

    public sealed class CeilingLine
    {
        public VertexSpot V1 { get; } = new();
        public CeilingSector? FrontSector { get; set; }
    }

    public sealed class SectorSpecial
    {
        public int Damage { get; set; }
    }

    public sealed class CeilingThinker
    {
        public required CeilingSector Sector { get; init; }
        public int Type { get; set; }
        public double BottomHeight { get; set; }
        public double TopHeight { get; set; }
        public double Speed { get; set; }
        public double Speed1 { get; set; }
        public double Speed2 { get; set; }
        public int Crush { get; set; } = -1;
        public int CrushMode { get; set; } = CrushDoom;
        public int Silent { get; set; }
        public int Direction { get; set; }
        public int OldDirection { get; set; }
        public int Tag { get; set; }
        public string Texture { get; set; } = "";
        public SectorSpecial NewSpecial { get; set; } = new();
    }

    public sealed class CreateCeilingContext
    {
        public Func<CeilingSector, bool>? PlaneMovingCeiling { get; init; }
        public Func<CeilingSector, double, double, int, CeilingThinker>? CreateThinker { get; init; }
        public Func<CeilingSector, VertexSpot?, double>? FindHighestFloorPoint { get; init; }
        public Func<CeilingSector, VertexSpot?, double>? FindHighestCeilingSurrounding { get; init; }
        public Func<CeilingSector, VertexSpot?, double>? FindHighestFloorSurrounding { get; init; }
        public Func<CeilingSector, VertexSpot?, double>? FindNextLowestCeiling { get; init; }
        public Func<CeilingSector, VertexSpot?, double>? FindNextHighestCeiling { get; init; }
        public Func<CeilingSector, VertexSpot?, double>? FindLowestCeilingSurrounding { get; init; }
        public Func<CeilingSector, double>? FindShortestUpperAround { get; init; }
        public Func<CeilingSector, double, CeilingSector?>? FindModelFloorSector { get; init; }
        public Func<CeilingSector, double, CeilingSector?>? FindModelCeilingSector { get; init; }
        public Action<CeilingThinker>? PlayCeilingSound { get; init; }
        public Action<CeilingThinker, bool>? StopInterpolation { get; init; }
    }

    public static bool CreateCeiling(
        CreateCeilingContext ctx,
        CeilingSector sec,
        int type,
        CeilingLine? line,
        int tag,
        double speed,
        double speed2,
        double height,
        int crush,
        int silent,
        int change,
        int hexencrush)
    {
        if (ctx.PlaneMovingCeiling?.Invoke(sec) == true)
            return false;

        var ceiling = ctx.CreateThinker?.Invoke(sec, speed, speed2, silent & ~4);
        if (ceiling == null)
            return false;

        var spot = sec.Lines.Count > 0 ? sec.Lines[0].V1 : new VertexSpot();
        double targheight = 0;

        switch (type)
        {
            case CeilCrushAndRaise:
            case CeilCrushRaiseAndStay:
                ceiling.TopHeight = sec.CeilingPlaneD;
                goto case CeilLowerAndCrush;
            case CeilLowerAndCrush:
                targheight = ctx.FindHighestFloorPoint?.Invoke(sec, spot) ?? 0;
                targheight += height;
                ceiling.BottomHeight = sec.PointToDist?.Invoke(spot.X, spot.Y, targheight) ?? 0;
                ceiling.Direction = -1;
                break;

            case CeilRaiseToHighest:
                targheight = ctx.FindHighestCeilingSurrounding?.Invoke(sec, spot) ?? 0;
                ceiling.TopHeight = sec.PointToDist?.Invoke(spot.X, spot.Y, targheight) ?? 0;
                ceiling.Direction = 1;
                break;

            case CeilLowerByValue:
                targheight = (sec.CeilingZAtPoint?.Invoke(spot.X, spot.Y) ?? 0) - height;
                ceiling.BottomHeight = sec.PointToDist?.Invoke(spot.X, spot.Y, targheight) ?? 0;
                ceiling.Direction = -1;
                break;

            case CeilRaiseByValue:
                targheight = (sec.CeilingZAtPoint?.Invoke(spot.X, spot.Y) ?? 0) + height;
                ceiling.TopHeight = sec.PointToDist?.Invoke(spot.X, spot.Y, targheight) ?? 0;
                ceiling.Direction = 1;
                break;

            case CeilMoveToValue:
            {
                var z = sec.CeilingZAtPoint?.Invoke(spot.X, spot.Y) ?? 0;
                var diff = height - z;
                targheight = height;
                if (diff < 0)
                {
                    ceiling.BottomHeight = sec.PointToDist?.Invoke(spot.X, spot.Y, height) ?? 0;
                    ceiling.Direction = -1;
                }
                else
                {
                    ceiling.TopHeight = sec.PointToDist?.Invoke(spot.X, spot.Y, height) ?? 0;
                    ceiling.Direction = 1;
                }
                break;
            }

            case CeilLowerToHighestFloor:
                targheight = (ctx.FindHighestFloorSurrounding?.Invoke(sec, spot) ?? 0) + height;
                ceiling.BottomHeight = sec.PointToDist?.Invoke(spot.X, spot.Y, targheight) ?? 0;
                ceiling.Direction = -1;
                break;

            case CeilRaiseToHighestFloor:
                targheight = ctx.FindHighestFloorSurrounding?.Invoke(sec, spot) ?? 0;
                ceiling.TopHeight = sec.PointToDist?.Invoke(spot.X, spot.Y, targheight) ?? 0;
                ceiling.Direction = 1;
                break;

            case CeilLowerInstant:
                targheight = (sec.CeilingZAtPoint?.Invoke(spot.X, spot.Y) ?? 0) - height;
                ceiling.BottomHeight = sec.PointToDist?.Invoke(spot.X, spot.Y, targheight) ?? 0;
                ceiling.Direction = -1;
                ceiling.Speed = height;
                break;

            case CeilRaiseInstant:
                targheight = (sec.CeilingZAtPoint?.Invoke(spot.X, spot.Y) ?? 0) + height;
                ceiling.TopHeight = sec.PointToDist?.Invoke(spot.X, spot.Y, targheight) ?? 0;
                ceiling.Direction = 1;
                ceiling.Speed = height;
                break;

            case CeilLowerToNearest:
                targheight = ctx.FindNextLowestCeiling?.Invoke(sec, spot) ?? 0;
                ceiling.BottomHeight = sec.PointToDist?.Invoke(spot.X, spot.Y, targheight) ?? 0;
                ceiling.Direction = -1;
                break;

            case CeilRaiseToNearest:
                targheight = ctx.FindNextHighestCeiling?.Invoke(sec, spot) ?? 0;
                ceiling.TopHeight = sec.PointToDist?.Invoke(spot.X, spot.Y, targheight) ?? 0;
                ceiling.Direction = 1;
                break;

            case CeilLowerToLowest:
                targheight = ctx.FindLowestCeilingSurrounding?.Invoke(sec, spot) ?? 0;
                ceiling.BottomHeight = sec.PointToDist?.Invoke(spot.X, spot.Y, targheight) ?? 0;
                ceiling.Direction = -1;
                break;

            case CeilRaiseToLowest:
                targheight = ctx.FindLowestCeilingSurrounding?.Invoke(sec, spot) ?? 0;
                ceiling.TopHeight = sec.PointToDist?.Invoke(spot.X, spot.Y, targheight) ?? 0;
                ceiling.Direction = 1;
                break;

            case CeilLowerToFloor:
                targheight = (ctx.FindHighestFloorPoint?.Invoke(sec, spot) ?? 0) + height;
                ceiling.BottomHeight = sec.PointToDist?.Invoke(spot.X, spot.Y, targheight) ?? 0;
                ceiling.Direction = -1;
                break;

            case CeilRaiseToFloor:
                targheight = (ctx.FindHighestFloorPoint?.Invoke(sec, spot) ?? 0) + height;
                ceiling.TopHeight = sec.PointToDist?.Invoke(spot.X, spot.Y, targheight) ?? 0;
                ceiling.Direction = 1;
                break;

            case CeilLowerToHighest:
                targheight = ctx.FindHighestCeilingSurrounding?.Invoke(sec, spot) ?? 0;
                ceiling.BottomHeight = sec.PointToDist?.Invoke(spot.X, spot.Y, targheight) ?? 0;
                ceiling.Direction = -1;
                break;

            case CeilLowerByTexture:
                targheight = (sec.CeilingZAtPoint?.Invoke(spot.X, spot.Y) ?? 0)
                    - (ctx.FindShortestUpperAround?.Invoke(sec) ?? 0);
                ceiling.BottomHeight = sec.PointToDist?.Invoke(spot.X, spot.Y, targheight) ?? 0;
                ceiling.Direction = -1;
                break;

            case CeilRaiseByTexture:
                targheight = (sec.CeilingZAtPoint?.Invoke(spot.X, spot.Y) ?? 0)
                    + (ctx.FindShortestUpperAround?.Invoke(sec) ?? 0);
                ceiling.TopHeight = sec.PointToDist?.Invoke(spot.X, spot.Y, targheight) ?? 0;
                ceiling.Direction = 1;
                break;
        }

        ceiling.Tag = tag;
        ceiling.Type = type;
        ceiling.Crush = crush;
        ceiling.CrushMode = hexencrush;

        double movedist;
        if (ceiling.Direction < 0)
            movedist = sec.CeilingPlaneD - ceiling.BottomHeight;
        else
            movedist = ceiling.TopHeight - sec.CeilingPlaneD;

        if (ceiling.Speed >= movedist)
        {
            ctx.StopInterpolation?.Invoke(ceiling, true);
            if ((silent & 4) != 0)
                ceiling.Silent = 2;
        }

        if ((change & 3) != 0)
        {
            if ((change & 4) != 0)
            {
                var modelsec = (type == CeilRaiseToFloor || type == CeilLowerToFloor)
                    ? ctx.FindModelFloorSector?.Invoke(sec, targheight)
                    : ctx.FindModelCeilingSector?.Invoke(sec, targheight);
                if (modelsec != null)
                {
                    ceiling.Texture = modelsec.CeilingTexture;
                    ApplyChangeType(ceiling, sec, line, change);
                }
            }
            else if (line != null)
            {
                ceiling.Texture = line.FrontSector?.CeilingTexture ?? "";
                ApplyChangeType(ceiling, sec, line, change);
            }
        }

        ctx.PlayCeilingSound?.Invoke(ceiling);
        return true;
    }

    static void ApplyChangeType(CeilingThinker ceiling, CeilingSector sec, CeilingLine? line, int change)
    {
        switch (change & 3)
        {
            case 1:
                ceiling.NewSpecial = new SectorSpecial();
                ceiling.Type = GenCeilingChg0;
                break;
            case 2:
                if (line?.FrontSector != null)
                    ceiling.NewSpecial = line.FrontSector.Special;
                ceiling.Type = GenCeilingChgT;
                break;
            case 3:
                ceiling.Type = GenCeilingChg;
                break;
        }
    }
}
