namespace HCDE.Playsim;

/// <summary><c>FLevelLocals::CreateFloor</c> from <c>a_floor.cpp</c> (281–538).</summary>
public static class VmAFloorCreate
{
    public const int FloorLowerToLowest = 0;
    public const int FloorLowerToNearest = 1;
    public const int FloorLowerToHighest = 2;
    public const int FloorLowerByValue = 3;
    public const int FloorRaiseByValue = 4;
    public const int FloorRaiseToHighest = 5;
    public const int FloorRaiseToNearest = 6;
    public const int FloorRaiseAndCrush = 7;
    public const int FloorRaiseAndCrushDoom = 8;
    public const int FloorLowerInstant = 10;
    public const int FloorRaiseInstant = 11;
    public const int FloorMoveToValue = 12;
    public const int FloorRaiseToLowestCeiling = 13;
    public const int FloorRaiseByTexture = 14;
    public const int FloorLowerAndChange = 15;
    public const int FloorRaiseAndChange = 16;
    public const int FloorRaiseToLowest = 17;
    public const int FloorRaiseToCeiling = 18;
    public const int FloorLowerToLowestCeiling = 19;
    public const int FloorLowerByTexture = 20;
    public const int FloorLowerToCeiling = 21;
    public const int CompatfSilentInstantFloors = 1;
    public const int SpacUse = 1;
    public const int SpacPush = 1 << 1;

    public sealed class VertexSpot
    {
        public double X { get; set; }
        public double Y { get; set; }
    }

    public sealed class FloorSector
    {
        public double FloorPlaneD { get; set; }
        public VertexSpot Centerspot { get; } = new();
        public string FloorTexture { get; set; } = "";
        public VmAFloorTickElevator.SectorSpecial Special { get; set; } = new();
        public object? FloorData { get; set; }
        public Func<double, double, double>? CenterFloor { get; set; }
        public Func<double, double, double, double>? PointToDist { get; set; }
    }

    public sealed class FloorLine
    {
        public FloorSector? FrontSector { get; set; }
        public FloorSector? BackSector { get; set; }
        public int Activation { get; set; }
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
        public string Texture { get; set; } = "";
        public VmAFloorTickElevator.SectorSpecial NewSpecial { get; set; } = new();
    }

    public sealed class CreateFloorContext
    {
        public int CompatFlags { get; init; }
        public Func<FloorSector, bool>? PlaneMovingFloor { get; init; }
        public Func<FloorSector, FloorThinker>? CreateThinker { get; init; }
        public Func<FloorSector, VertexSpot?, double>? FindHighestFloorSurrounding { get; init; }
        public Func<FloorSector, VertexSpot?, double>? FindLowestFloorSurrounding { get; init; }
        public Func<FloorSector, VertexSpot?, double>? FindNextLowestFloor { get; init; }
        public Func<FloorSector, VertexSpot?, double>? FindNextHighestFloor { get; init; }
        public Func<FloorSector, VertexSpot?, double>? FindLowestCeilingSurrounding { get; init; }
        public Func<FloorSector, VertexSpot?, double>? FindLowestCeilingPoint { get; init; }
        public Func<FloorSector, double>? FindShortestTextureAround { get; init; }
        public Func<FloorSector, VertexSpot?>? FindHighestFloorPoint { get; init; }
        public Func<FloorSector, double, FloorSector?>? FindModelFloorSector { get; init; }
        public Func<FloorSector, double, FloorSector?>? FindModelCeilingSector { get; init; }
        public Action<FloorThinker>? StartFloorSound { get; init; }
        public Action<FloorThinker, bool>? StopInterpolation { get; init; }
        public Action<FloorThinker, FloorSector, int>? SetFloorChangeType { get; init; }
        public Action<FloorSector, string>? SetFloorTexture { get; init; }
        public Action<FloorSector>? ClearSpecial { get; init; }
        public Action<FloorSector, FloorSector>? TransferSpecial { get; init; }
        public Func<FloorSector, double, double, double, double>? ZatPointDist { get; init; }
    }

    public static bool CreateFloor(
        CreateFloorContext ctx,
        FloorSector sec,
        int floortype,
        FloorLine? line,
        double speed,
        double height,
        int crush,
        int change,
        bool hexencrush,
        bool hereticlower)
    {
        if (ctx.PlaneMovingFloor?.Invoke(sec) == true)
            return false;

        var floor = ctx.CreateThinker?.Invoke(sec);
        if (floor == null)
            return false;

        floor.Type = floortype;
        floor.Crush = crush;
        floor.Hexencrush = hexencrush;
        floor.Speed = speed;
        floor.ResetCount = 0;
        floor.OrgDist = sec.FloorPlaneD;
        floor.Instant = false;

        VertexSpot? spot = sec.Centerspot;
        double newheight;

        switch (floortype)
        {
            case FloorLowerToHighest:
                floor.Direction = -1;
                newheight = ctx.FindHighestFloorSurrounding?.Invoke(sec, spot) ?? 0;
                floor.FloorDestDist = sec.PointToDist?.Invoke(spot!.X, spot.Y, newheight) ?? 0;
                if (hereticlower || floor.FloorDestDist != sec.FloorPlaneD)
                    floor.FloorDestDist = sec.PointToDist?.Invoke(spot.X, spot.Y, newheight + height) ?? 0;
                break;
            case FloorLowerToLowest:
                floor.Direction = -1;
                newheight = ctx.FindLowestFloorSurrounding?.Invoke(sec, spot) ?? 0;
                floor.FloorDestDist = sec.PointToDist?.Invoke(spot!.X, spot.Y, newheight) ?? 0;
                break;
            case FloorLowerToNearest:
                floor.Direction = -1;
                newheight = ctx.FindNextLowestFloor?.Invoke(sec, spot) ?? 0;
                floor.FloorDestDist = sec.PointToDist?.Invoke(spot!.X, spot.Y, newheight) ?? 0;
                break;
            case FloorLowerInstant:
                floor.Speed = height;
                goto case FloorLowerByValue;
            case FloorLowerByValue:
                floor.Direction = -1;
                newheight = (sec.CenterFloor?.Invoke(sec.Centerspot.X, sec.Centerspot.Y) ?? 0) - height;
                floor.FloorDestDist = sec.PointToDist?.Invoke(sec.Centerspot.X, sec.Centerspot.Y, newheight) ?? 0;
                break;
            case FloorRaiseInstant:
                floor.Speed = height;
                goto case FloorRaiseByValue;
            case FloorRaiseByValue:
                floor.Direction = 1;
                newheight = (sec.CenterFloor?.Invoke(sec.Centerspot.X, sec.Centerspot.Y) ?? 0) + height;
                floor.FloorDestDist = sec.PointToDist?.Invoke(sec.Centerspot.X, sec.Centerspot.Y, newheight) ?? 0;
                break;
            case FloorMoveToValue:
                spot = ctx.FindHighestFloorPoint?.Invoke(sec) ?? sec.Centerspot;
                floor.FloorDestDist = sec.PointToDist?.Invoke(spot.X, spot.Y, height) ?? 0;
                floor.Direction = floor.FloorDestDist > sec.FloorPlaneD ? -1 : 1;
                break;
            case FloorRaiseAndCrushDoom:
                height = 8;
                goto case FloorRaiseToLowestCeiling;
            case FloorRaiseToLowestCeiling:
                floor.Direction = 1;
                newheight = (ctx.FindLowestCeilingSurrounding?.Invoke(sec, spot) ?? 0) - height;
                var ceilingheight = ctx.FindLowestCeilingPoint?.Invoke(sec, spot) ?? 0;
                floor.FloorDestDist = sec.PointToDist?.Invoke(spot!.X, spot.Y, newheight) ?? 0;
                if ((ctx.ZatPointDist?.Invoke(sec, spot!.X, spot.Y, floor.FloorDestDist) ?? 0) > ceilingheight)
                    floor.FloorDestDist = sec.PointToDist?.Invoke(spot.X, spot.Y, ceilingheight - height) ?? 0;
                break;
            case FloorRaiseToHighest:
                floor.Direction = 1;
                newheight = ctx.FindHighestFloorSurrounding?.Invoke(sec, spot) ?? 0;
                floor.FloorDestDist = sec.PointToDist?.Invoke(spot!.X, spot.Y, newheight) ?? 0;
                break;
            case FloorRaiseToNearest:
                floor.Direction = 1;
                newheight = ctx.FindNextHighestFloor?.Invoke(sec, spot) ?? 0;
                floor.FloorDestDist = sec.PointToDist?.Invoke(spot!.X, spot.Y, newheight) ?? 0;
                break;
            case FloorRaiseToLowest:
                floor.Direction = 1;
                newheight = ctx.FindLowestFloorSurrounding?.Invoke(sec, spot) ?? 0;
                floor.FloorDestDist = sec.PointToDist?.Invoke(spot!.X, spot.Y, newheight) ?? 0;
                break;
            case FloorRaiseAndCrush:
                floor.Direction = 1;
                newheight = (ctx.FindLowestCeilingPoint?.Invoke(sec, spot) ?? 0) - 8;
                floor.FloorDestDist = sec.PointToDist?.Invoke(spot!.X, spot.Y, newheight) ?? 0;
                break;
            case FloorRaiseToCeiling:
                floor.Direction = 1;
                newheight = (ctx.FindLowestCeilingPoint?.Invoke(sec, spot) ?? 0) - height;
                floor.FloorDestDist = sec.PointToDist?.Invoke(spot!.X, spot.Y, newheight) ?? 0;
                break;
            case FloorLowerToLowestCeiling:
                floor.Direction = -1;
                newheight = ctx.FindLowestCeilingSurrounding?.Invoke(sec, spot) ?? 0;
                floor.FloorDestDist = sec.PointToDist?.Invoke(spot!.X, spot.Y, newheight) ?? 0;
                break;
            case FloorLowerByTexture:
                floor.Direction = -1;
                newheight = (sec.CenterFloor?.Invoke(sec.Centerspot.X, sec.Centerspot.Y) ?? 0) - (ctx.FindShortestTextureAround?.Invoke(sec) ?? 0);
                floor.FloorDestDist = sec.PointToDist?.Invoke(sec.Centerspot.X, sec.Centerspot.Y, newheight) ?? 0;
                break;
            case FloorLowerToCeiling:
                floor.Direction = -1;
                newheight = (ctx.FindLowestCeilingPoint?.Invoke(sec, spot) ?? 0) - height;
                floor.FloorDestDist = sec.PointToDist?.Invoke(spot!.X, spot.Y, newheight) ?? 0;
                break;
            case FloorRaiseByTexture:
                floor.Direction = 1;
                newheight = (sec.CenterFloor?.Invoke(sec.Centerspot.X, sec.Centerspot.Y) ?? 0) + (ctx.FindShortestTextureAround?.Invoke(sec) ?? 0);
                floor.FloorDestDist = sec.PointToDist?.Invoke(sec.Centerspot.X, sec.Centerspot.Y, newheight) ?? 0;
                break;
            case FloorRaiseAndChange:
                floor.Direction = 1;
                newheight = (sec.CenterFloor?.Invoke(sec.Centerspot.X, sec.Centerspot.Y) ?? 0) + height;
                floor.FloorDestDist = sec.PointToDist?.Invoke(sec.Centerspot.X, sec.Centerspot.Y, newheight) ?? 0;
                if (line?.FrontSector != null)
                {
                    ctx.SetFloorTexture?.Invoke(sec, line.FrontSector.FloorTexture);
                    ctx.TransferSpecial?.Invoke(sec, line.FrontSector);
                }
                else
                    ctx.ClearSpecial?.Invoke(sec);
                break;
            case FloorLowerAndChange:
                floor.Direction = -1;
                newheight = ctx.FindLowestFloorSurrounding?.Invoke(sec, spot) ?? 0;
                floor.FloorDestDist = sec.PointToDist?.Invoke(spot!.X, spot.Y, newheight) ?? 0;
                floor.Texture = sec.FloorTexture;
                floor.NewSpecial = new VmAFloorTickElevator.SectorSpecial { Tag = sec.Special.Tag };
                var modelsec = ctx.FindModelFloorSector?.Invoke(sec, newheight);
                if (modelsec != null)
                {
                    floor.Texture = modelsec.FloorTexture;
                    floor.NewSpecial = new VmAFloorTickElevator.SectorSpecial { Tag = modelsec.Special.Tag };
                }
                break;
        }

        var silent = false;
        if ((floor.Direction > 0 && floor.FloorDestDist > sec.FloorPlaneD) ||
            (floor.Direction < 0 && floor.FloorDestDist < sec.FloorPlaneD) ||
            floor.Speed >= Math.Abs(sec.FloorPlaneD - floor.FloorDestDist))
        {
            ctx.StopInterpolation?.Invoke(floor, true);
            floor.Instant = true;
            if ((ctx.CompatFlags & CompatfSilentInstantFloors) == 0)
            {
                if (line == null || (line.Activation & (SpacUse | SpacPush)) == 0 || line.BackSector != sec)
                    silent = true;
            }
        }

        if (!silent)
            ctx.StartFloorSound?.Invoke(floor);

        if ((change & 3) != 0)
        {
            if ((change & 4) != 0)
            {
                FloorSector? model = floortype is FloorRaiseToLowestCeiling or FloorLowerToLowestCeiling or FloorRaiseToCeiling or FloorLowerToCeiling
                    ? ctx.FindModelCeilingSector?.Invoke(sec, -floor.FloorDestDist)
                    : ctx.FindModelFloorSector?.Invoke(sec, -floor.FloorDestDist);
                if (model != null)
                    ctx.SetFloorChangeType?.Invoke(floor, model, change);
            }
            else if (line?.FrontSector != null)
                ctx.SetFloorChangeType?.Invoke(floor, line.FrontSector, change);
        }

        return true;
    }
}
