namespace HCDE.Playsim;

/// <summary>Sector terrain, portal plane, height-at-point, and friction helpers from <c>p_sectors.cpp</c> (968–1159).</summary>
public static class VmPSectorTerrainPortalAt
{
    public const int Floor = 0;
    public const int Ceiling = 1;
    public const int PortsLinkedPortal = 3;
    public const uint PlanefObstructed = 128;
    public const uint SecfFriction = 16;
    public const int Ffcf3drestrict = 8;
    public const int FfcfNoportals = 16;
    public const int FfExists = 0x1;
    public const int FfSolid = 0x2;
    public const double OrigFriction = 0.90625;
    public const double OrigFrictionFactor = 2048.0 / 65536.0;

    public sealed class Plane
    {
        public uint Flags { get; set; }
        public Func<double, double, double> ZAtPoint { get; set; } = (_, _) => 0;
    }

    public sealed class FFloor
    {
        public int Flags { get; set; }
        public Plane Top { get; } = new();
        public Plane Bottom { get; } = new();
    }

    public sealed class SectorXFloor
    {
        public List<FFloor> FFloors { get; } = new();
    }

    public sealed class TerrainDef
    {
        public double Friction { get; set; }
        public double MoveFactor { get; set; }
    }

    public sealed class MapLevel
    {
        public Func<double, double, MapSector?> PointInSector { get; set; } = (_, _) => null;
    }

    public sealed class MapSector
    {
        public MapLevel? Level { get; set; }
        public SectorXFloor XFloor { get; } = new();
        public Plane FloorPlane { get; } = new();
        public Plane CeilingPlane { get; } = new();
        public PlaneSlot[] Planes { get; } = { new(), new() };
        public int[] TerrainNum { get; } = { -1, -1 };
        public uint Flags { get; set; }
        public double Friction { get; set; }
        public double MoveFactor { get; set; }

        public int GetTexture(int pos) => Planes[pos].Texture;
        public int GetTerrain(int pos) => GetTerrainStatic(this, pos);
        public int GetPortalType(int plane) => Planes[plane].PortalType;
        public double GetPortalPlaneZ(int plane) => Planes[plane].PortalPlaneZ;
        public double GetPlaneTexZ(int plane) => Planes[plane].TexZ;
        public bool PortalBlocksMovement(int plane) => Planes[plane].PortalBlocksMovement;
        public (double X, double Y) GetPortalDisplacement(int plane) => Planes[plane].PortalDisplacement;
    }

    public sealed class PlaneSlot
    {
        public uint Flags { get; set; }
        public int Texture { get; set; }
        public int PortalType { get; set; }
        public double PortalPlaneZ { get; set; }
        public double TexZ { get; set; }
        public bool PortalBlocksMovement { get; set; }
        public (double X, double Y) PortalDisplacement { get; set; }
    }

    public static int[] TerrainTypes { get; set; } = Array.Empty<int>();
    public static TerrainDef[] Terrains { get; set; } = Array.Empty<TerrainDef>();

    public static int GetTerrain(MapSector sector, int pos) =>
        sector.TerrainNum[pos] >= 0 ? sector.TerrainNum[pos] : TerrainTypes[sector.GetTexture(pos)];

    public static int GetTerrainStatic(MapSector sector, int pos) => GetTerrain(sector, pos);

    public static TerrainDef GetFloorTerrainS(MapSector sec, int pos) => Terrains[GetTerrain(sec, pos)];

    public static void CheckPortalPlane(MapSector sector, int plane)
    {
        if (sector.GetPortalType(plane) == PortsLinkedPortal)
        {
            var portalh = sector.GetPortalPlaneZ(plane);
            var planeh = sector.GetPlaneTexZ(plane);
            var obstructed = PlanefObstructed * (uint)(plane == Floor ? (planeh > portalh ? 1 : 0) : (planeh < portalh ? 1 : 0));
            sector.Planes[plane].Flags = (sector.Planes[plane].Flags & ~PlanefObstructed) | obstructed;
        }
    }

    public static double HighestCeilingAt(MapSector check, double x, double y, out MapSector? resultSec)
    {
        var planeheight = double.NegativeInfinity;
        var posX = x;
        var posY = y;

        while (!check.PortalBlocksMovement(Ceiling) && planeheight < check.GetPortalPlaneZ(Ceiling))
        {
            var disp = check.GetPortalDisplacement(Ceiling);
            posX += disp.X;
            posY += disp.Y;
            planeheight = check.GetPortalPlaneZ(Ceiling);
            check = check.Level?.PointInSector(posX, posY) ?? check;
        }

        resultSec = check;
        return check.CeilingPlane.ZAtPoint(posX, posY);
    }

    public static double LowestFloorAt(MapSector check, double x, double y, out MapSector? resultSec)
    {
        var planeheight = double.PositiveInfinity;
        var posX = x;
        var posY = y;

        while (!check.PortalBlocksMovement(Floor) && planeheight > check.GetPortalPlaneZ(Floor))
        {
            var disp = check.GetPortalDisplacement(Floor);
            posX += disp.X;
            posY += disp.Y;
            planeheight = check.GetPortalPlaneZ(Ceiling);
            check = check.Level?.PointInSector(posX, posY) ?? check;
        }

        resultSec = check;
        return check.FloorPlane.ZAtPoint(posX, posY);
    }

    public static double NextHighestCeilingAt(
        MapSector sec,
        double x,
        double y,
        double bottomz,
        double topz,
        int flags,
        out MapSector? resultSec,
        out FFloor? resultFfloor)
    {
        var planeheight = double.NegativeInfinity;
        resultSec = null;
        resultFfloor = null;

        while (true)
        {
            var realceil = sec.CeilingPlane.ZAtPoint(x, y);
            for (var i = sec.XFloor.FFloors.Count - 1; i >= 0; --i)
            {
                var rover = sec.XFloor.FFloors[i];
                if ((rover.Flags & FfSolid) == 0 || (rover.Flags & FfExists) == 0)
                    continue;

                var ffBottom = rover.Bottom.ZAtPoint(x, y);
                var ffTop = rover.Top.ZAtPoint(x, y);
                var delta1 = bottomz - (ffBottom + (ffTop - ffBottom) / 2);
                var delta2 = topz - (ffBottom + (ffTop - ffBottom) / 2);

                if (ffBottom < realceil && Math.Abs(delta1) > Math.Abs(delta2))
                {
                    resultSec = sec;
                    resultFfloor = rover;
                    return ffBottom;
                }
            }

            if ((flags & FfcfNoportals) != 0 || sec.PortalBlocksMovement(Ceiling)
                || planeheight >= sec.GetPortalPlaneZ(Ceiling))
            {
                resultFfloor = null;
                resultSec = sec;
                return realceil;
            }

            var pos = sec.GetPortalDisplacement(Ceiling);
            x += pos.X;
            y += pos.Y;
            planeheight = sec.GetPortalPlaneZ(Ceiling);
            sec = sec.Level?.PointInSector(x, y) ?? sec;
        }
    }

    public static double NextLowestFloorAt(
        MapSector sec,
        double x,
        double y,
        double z,
        int flags,
        double steph,
        out MapSector? resultSec,
        out FFloor? resultFfloor)
    {
        var planeheight = double.PositiveInfinity;
        resultSec = null;
        resultFfloor = null;

        while (true)
        {
            var numff = sec.XFloor.FFloors.Count;
            var realfloor = sec.FloorPlane.ZAtPoint(x, y);
            for (var i = 0; i < numff; ++i)
            {
                var ff = sec.XFloor.FFloors[i];
                if ((ff.Flags & (FfExists | FfSolid)) != (FfExists | FfSolid))
                    continue;

                var ffz = ff.Top.ZAtPoint(x, y);
                var ffb = ff.Bottom.ZAtPoint(x, y);

                if (ffz > realfloor && (z >= ffz || ((flags & Ffcf3drestrict) == 0 && ffb < z && ffz < z + steph)))
                {
                    resultSec = sec;
                    resultFfloor = ff;
                    return ffz;
                }
            }

            if ((flags & FfcfNoportals) != 0 || sec.PortalBlocksMovement(Floor)
                || planeheight <= sec.GetPortalPlaneZ(Floor))
            {
                resultFfloor = null;
                resultSec = sec;
                return realfloor;
            }

            var pos = sec.GetPortalDisplacement(Floor);
            x += pos.X;
            y += pos.Y;
            planeheight = sec.GetPortalPlaneZ(Floor);
            sec = sec.Level?.PointInSector(x, y) ?? sec;
        }
    }

    public static double GetFriction(MapSector self, int plane, out double moveFac)
    {
        if ((self.Flags & SecfFriction) != 0)
        {
            moveFac = self.MoveFactor;
            return self.Friction;
        }

        var terrain = Terrains[self.GetTerrain(plane)];
        if (terrain.Friction != 0)
        {
            moveFac = terrain.MoveFactor;
            return terrain.Friction;
        }

        moveFac = OrigFrictionFactor;
        return OrigFriction;
    }
}
