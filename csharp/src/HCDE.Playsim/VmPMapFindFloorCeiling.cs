namespace HCDE.Playsim;

/// <summary><c>FindRefPoint</c>, <c>PIT_FindFloorCeiling</c>, <c>P_GetFloorCeilingZ</c>, and <c>P_FindFloorCeiling</c> from <c>p_map.cpp</c> (214–420).</summary>
public static class VmPMapFindFloorCeiling
{
    public const int FfcfOnlyspawnpos = 1;
    public const int FfcfSamesector = 2;
    public const int FfcfNofloor = 32;
    public const int FfcfNoceiling = 64;
    public const double LineOpenMin = -3.402823466e38;

    public sealed class Vec2
    {
        public double X { get; set; }
        public double Y { get; set; }
    }

    public sealed class MapLine
    {
        public Vec2 V1 { get; } = new();
        public Vec2 V2 { get; } = new();
        public MapSector? FrontSector { get; set; }
        public MapSector? BackSector { get; set; }
        public int Index { get; set; }
        public int FrontPortalGroup { get; set; }

        public (double X, double Y) Delta() => (V2.X - V1.X, V2.Y - V1.Y);
    }

    public sealed class MapSector
    {
        public SecPlane FloorPlane { get; } = new();
        public SecPlane CeilingPlane { get; } = new();
        public SectorXFloor XFloor { get; } = new();
        public int PortalGroup { get; set; }
        public bool PortalBlocksCeiling { get; set; }
        public bool PortalBlocksFloor { get; set; }
        public int[] TerrainNum { get; } = { -1, -1 };
        public int[] Textures { get; } = { 0, 0 };

        public int GetTexture(int pos) => Textures[pos];

        public int GetTerrain(int pos)
        {
            if (TerrainNum[pos] >= 0)
                return TerrainNum[pos];
            var tex = GetTexture(pos);
            return tex >= 0 && tex < TerrainTypeTable.Length ? TerrainTypeTable[tex] : 0;
        }
    }

    public static int[] TerrainTypeTable { get; set; } = Array.Empty<int>();

    public sealed class SecPlane
    {
        public bool IsSlope { get; set; }
        public Func<double, double, double> ZAtPoint { get; set; } = (_, _) => 0;
    }

    public sealed class SectorXFloor
    {
        public int Count => FFloors.Count;
        public List<object> FFloors { get; } = new();
    }

    public sealed class FFloorRef
    {
        public object? TopTexture { get; set; }
        public object? BottomTexture { get; set; }
        public MapSector? Model { get; set; }
        public int TopIsceiling { get; set; }
    }

    public sealed class LineOpening
    {
        public double Top { get; set; }
        public double Bottom { get; set; }
        public double LowFloor { get; set; } = LineOpenMin;
        public MapSector? TopSec { get; set; }
        public MapSector? BottomSec { get; set; }
        public bool TouchMidtex { get; set; }
        public bool AboveMidtex { get; set; }
    }

    public sealed class CheckPosition
    {
        public MapActor? Thing { get; set; }
        public Vec2 Pos { get; } = new();
        public double PosZ { get; set; }
        public MapSector? Sector { get; set; }
        public double CeilingZ { get; set; }
        public double FloorZ { get; set; }
        public double DropoffZ { get; set; }
        public bool DropoffIsPortal { get; set; }
        public object? FloorPic { get; set; }
        public object? CeilingPic { get; set; }
        public MapSector? FloorSector { get; set; }
        public MapSector? CeilingSector { get; set; }
        public int FloorTerrain { get; set; }
        public bool TouchMidtex { get; set; }
        public bool AboveMidtex { get; set; }
    }

    public sealed class MapActor
    {
        public Vec2 Pos { get; } = new();
        public MapSector? Sector { get; set; }
        public MapLevel? Level { get; set; }
        public double Z { get; set; }
        public double Height { get; set; } = 41;
        public double MaxStepHeight { get; set; } = 24;
        public double MaxDropOffHeight { get; set; } = 24;
        public double FloorZ { get; set; }
        public double CeilingZ { get; set; }
        public double DropoffZ { get; set; }
        public object? FloorPic { get; set; }
        public object? CeilingPic { get; set; }
        public MapSector? FloorSector { get; set; }
        public MapSector? CeilingSector { get; set; }
        public int FloorTerrain { get; set; }
    }

    public sealed class MapLevel
    {
        public Func<double, double, MapSector?> PointInSector { get; set; } = (_, _) => null;
    }

    public sealed class BoundingBox
    {
        public double MinX { get; set; }
        public double MinY { get; set; }
        public double MaxX { get; set; }
        public double MaxY { get; set; }
    }

    public sealed class LineCheckResult
    {
        public MapLine? Line { get; set; }
        public double PosX { get; set; }
        public double PosY { get; set; }
        public int PortalFlags { get; set; }
    }

    public sealed class LineIterator
    {
        public bool StopUp { get; set; }
        public bool StopDown { get; set; }
        public BoundingBox Box { get; } = new();
        public Func<LineCheckResult, bool>? Next { get; set; }
    }

    public static bool FfcfVerbose { get; set; }

    public static Vec2 FindRefPoint(MapLine ld, double posX, double posY)
    {
        var front = ld.FrontSector!;
        var back = ld.BackSector!;
        if (front.FloorPlane.IsSlope || back.FloorPlane.IsSlope || front.CeilingPlane.IsSlope || back.CeilingPlane.IsSlope
            || back.XFloor.Count != 0 || front.XFloor.Count != 0
            || !front.PortalBlocksCeiling || !front.PortalBlocksFloor)
        {
            var v1 = ld.V1;
            var d = ld.Delta();
            var den = d.X * d.X + d.Y * d.Y;
            var r = den == 0 ? 0 : Math.Clamp(((posX - v1.X) * d.X + (posY - v1.Y) * d.Y) / den, 0, 1);
            return new Vec2 { X = v1.X + d.X * r, Y = v1.Y + d.Y * r };
        }

        return new Vec2 { X = posX, Y = posY };
    }

    public static bool PitFindFloorCeiling(
        LineIterator mit,
        LineCheckResult cres,
        BoundingBox box,
        CheckPosition tmf,
        int flags,
        Func<BoundingBox, MapLine, bool>? inRange = null,
        Func<BoundingBox, MapLine, int>? boxOnLineSide = null,
        Action<LineOpening, MapActor?, MapLine, Vec2, LineCheckResult, int>? lineOpening = null)
    {
        var ld = cres.Line!;
        if (inRange?.Invoke(box, ld) == false || boxOnLineSide?.Invoke(box, ld) != -1)
            return true;

        if (ld.BackSector is null)
            return true;

        var refpoint = FindRefPoint(ld, cres.PosX, cres.PosY);
        var open = new LineOpening();
        lineOpening?.Invoke(open, tmf.Thing, ld, refpoint, cres, flags);

        if ((flags & FfcfNoceiling) == 0)
        {
            if (open.Top < tmf.CeilingZ)
            {
                tmf.CeilingZ = open.Top;
                if (open.TopSec is not null)
                    tmf.CeilingSector = open.TopSec;
                mit.StopUp = true;
            }
        }

        if ((flags & FfcfNofloor) == 0)
        {
            if (open.Bottom > tmf.FloorZ)
            {
                tmf.FloorZ = open.Bottom;
                if (open.BottomSec is not null)
                    tmf.FloorSector = open.BottomSec;
                tmf.TouchMidtex = open.TouchMidtex;
                tmf.AboveMidtex = open.AboveMidtex;
                if (tmf.FloorZ > tmf.DropoffZ + tmf.Thing!.MaxDropOffHeight)
                    mit.StopDown = true;
            }
            else if (open.Bottom == tmf.FloorZ)
            {
                tmf.TouchMidtex |= open.TouchMidtex;
                tmf.AboveMidtex |= open.AboveMidtex;
            }

            if (open.LowFloor < tmf.DropoffZ && open.LowFloor > LineOpenMin)
            {
                tmf.DropoffZ = open.LowFloor;
                if (tmf.FloorZ > tmf.DropoffZ + tmf.Thing!.MaxDropOffHeight)
                    mit.StopDown = true;
            }
        }

        return true;
    }

    public delegate double NextHighestCeilingDelegate(
        MapSector sec, double x, double y, double bottomz, double topz, int flags, out MapSector? resultSec, out FFloorRef? resultFfloor);

    public delegate double NextLowestFloorDelegate(
        MapSector sec, double x, double y, double z, int flags, double steph, out MapSector? resultSec, out FFloorRef? resultFfloor);

    public static void GetFloorCeilingZ(
        CheckPosition tmf,
        int flags,
        NextHighestCeilingDelegate? nextHighestCeilingAt = null,
        NextLowestFloorDelegate? nextLowestFloorAt = null)
    {
        nextHighestCeilingAt ??= DefaultNextHighestCeiling;
        nextLowestFloorAt ??= DefaultNextLowestFloor;

        var sec = ((flags & FfcfSamesector) == 0 || tmf.Thing!.Sector is null)
            ? tmf.Thing!.Level!.PointInSector(tmf.Pos.X, tmf.Pos.Y)
            : tmf.Sector;
        FFloorRef? ffc;
        FFloorRef? fff;
        tmf.CeilingZ = nextHighestCeilingAt(
            sec!, tmf.Pos.X, tmf.Pos.Y, tmf.PosZ, tmf.PosZ + tmf.Thing!.Height, flags, out var ceilSec, out ffc);
        tmf.CeilingSector = ceilSec;
        tmf.FloorZ = tmf.DropoffZ = nextLowestFloorAt(
            sec!, tmf.Pos.X, tmf.Pos.Y, tmf.PosZ, flags, tmf.Thing!.MaxStepHeight, out var floorSec, out fff);
        tmf.FloorSector = floorSec;
        tmf.DropoffIsPortal = tmf.FloorSector != sec;

        if (fff is not null)
        {
            tmf.FloorPic = fff.TopTexture;
            tmf.FloorTerrain = fff.Model!.GetTerrain(fff.TopIsceiling);
        }
        else
        {
            tmf.FloorPic = tmf.FloorSector!.GetTexture(0);
            tmf.FloorTerrain = tmf.FloorSector.GetTerrain(0);
        }

        tmf.CeilingPic = ffc is not null ? ffc.BottomTexture : tmf.CeilingSector!.GetTexture(1);
        tmf.Sector = sec;
    }

    static double DefaultNextHighestCeiling(
        MapSector sec, double x, double y, double bottomz, double topz, int flags,
        out MapSector? resultSec, out FFloorRef? resultFfloor)
    {
        resultSec = sec;
        resultFfloor = null;
        return sec.CeilingPlane.ZAtPoint(x, y);
    }

    static double DefaultNextLowestFloor(
        MapSector sec, double x, double y, double z, int flags, double steph,
        out MapSector? resultSec, out FFloorRef? resultFfloor)
    {
        resultSec = sec;
        resultFfloor = null;
        return sec.FloorPlane.ZAtPoint(x, y);
    }

    public static void FindFloorCeiling(
        MapActor actor,
        int flags,
        Action<int>? bumpValidCount = null,
        NextHighestCeilingDelegate? nextHighestCeilingAt = null,
        NextLowestFloorDelegate? nextLowestFloorAt = null,
        Func<MapActor, LineIterator, int, bool>? iterateLines = null)
    {
        if ((flags & FfcfOnlyspawnpos) != 0)
            flags |= VmPSectorTerrainPortalAt.Ffcf3drestrict;

        var tmf = new CheckPosition { Thing = actor };
        tmf.Pos.X = actor.Pos.X;
        tmf.Pos.Y = actor.Pos.Y;
        tmf.PosZ = actor.Z;

        if ((flags & FfcfSamesector) != 0)
            tmf.Sector = actor.Sector;

        GetFloorCeilingZ(tmf, flags, nextHighestCeilingAt, nextLowestFloorAt);

        actor.FloorZ = tmf.FloorZ;
        actor.DropoffZ = tmf.DropoffZ;
        actor.CeilingZ = tmf.CeilingZ;
        actor.FloorPic = tmf.FloorPic;
        actor.FloorTerrain = tmf.FloorTerrain;
        actor.FloorSector = tmf.FloorSector;
        actor.CeilingPic = tmf.CeilingPic;
        actor.CeilingSector = tmf.CeilingSector;

        tmf.TouchMidtex = false;
        tmf.AboveMidtex = false;
        bumpValidCount?.Invoke(1);

        var mit = new LineIterator();
        if (actor.FloorSector == actor.Sector)
            mit.StopDown = true;
        if (actor.CeilingSector == actor.Sector)
            mit.StopUp = true;

        iterateLines?.Invoke(actor, mit, flags);

        if (tmf.TouchMidtex)
            tmf.DropoffZ = tmf.FloorZ;

        var usetmf = (flags & FfcfOnlyspawnpos) == 0 || (tmf.AboveMidtex && tmf.FloorZ <= actor.Z);

        if (usetmf || tmf.FloorSector!.PortalGroup != actor.Sector!.PortalGroup)
        {
            actor.FloorZ = tmf.FloorZ;
            actor.DropoffZ = tmf.DropoffZ;
            actor.FloorPic = tmf.FloorPic;
            actor.FloorTerrain = tmf.FloorTerrain;
            actor.FloorSector = tmf.FloorSector;
        }

        if (usetmf || tmf.CeilingSector!.PortalGroup != actor.Sector!.PortalGroup)
        {
            actor.CeilingZ = tmf.CeilingZ;
            actor.CeilingPic = tmf.CeilingPic;
            actor.CeilingSector = tmf.CeilingSector;
        }
    }
}
