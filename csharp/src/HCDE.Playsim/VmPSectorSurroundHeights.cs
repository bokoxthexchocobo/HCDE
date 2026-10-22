namespace HCDE.Playsim;

/// <summary>
/// Sector line geometry and surrounding height queries from <c>p_sectors.cpp</c> (168–447).
/// </summary>
public static class VmPSectorSurroundHeights
{
    public sealed class Vertex
    {
        public double X { get; set; }
        public double Y { get; set; }
    }

    public sealed class MapLine
    {
        public Vertex V1 { get; } = new();
        public Vertex V2 { get; } = new();
        public MapSector? FrontSector { get; set; }
        public MapSector? BackSector { get; set; }
    }

    public sealed class Plane
    {
        public Func<double, double, double> ZAtPoint { get; set; } = (_, _) => 0;
    }

    public sealed class MapSector
    {
        public List<MapLine> Lines { get; } = new();
        public Plane FloorPlane { get; } = new();
        public Plane CeilingPlane { get; } = new();
        public double FloorTexZ { get; set; }
        public double CeilingTexZ { get; set; }
        public int SectorIndex { get; set; }
        public VmLinkedSectorPlane? LinkedPlane { get; set; }

        public double GetPlaneTexZ(bool ceiling) => ceiling ? CeilingTexZ : FloorTexZ;

        public bool IsLinked(MapSector other, bool ceiling)
        {
            if (LinkedPlane is null)
                return false;
            return VmLinkedSectorMovement.IsLinked(LinkedPlane, other.SectorIndex, ceiling);
        }
    }

    public static MapSector? GetNextSector(MapLine line, MapSector from)
    {
        if (line.FrontSector == from)
            return line.BackSector;
        if (line.BackSector == from)
            return line.FrontSector;
        return null;
    }

    public static double FindLowestFloorSurrounding(MapSector sector, Vertex? spotOut = null)
    {
        if (sector.Lines.Count == 0)
            return sector.GetPlaneTexZ(ceiling: false);

        var spot = sector.Lines[0].V1;
        var floor = sector.FloorPlane.ZAtPoint(spot.X, spot.Y);

        foreach (var check in sector.Lines)
        {
            var other = GetNextSector(check, sector);
            if (other is null)
                continue;

            var ofloor = other.FloorPlane.ZAtPoint(check.V1.X, check.V1.Y);
            if (ofloor < floor && ofloor < sector.FloorPlane.ZAtPoint(check.V1.X, check.V1.Y))
            {
                floor = ofloor;
                spot = check.V1;
            }

            ofloor = other.FloorPlane.ZAtPoint(check.V2.X, check.V2.Y);
            if (ofloor < floor && ofloor < sector.FloorPlane.ZAtPoint(check.V2.X, check.V2.Y))
            {
                floor = ofloor;
                spot = check.V2;
            }
        }

        if (spotOut is not null)
        {
            spotOut.X = spot.X;
            spotOut.Y = spot.Y;
        }

        return floor;
    }

    public static double FindHighestFloorSurrounding(MapSector sector, Vertex? spotOut = null)
    {
        if (sector.Lines.Count == 0)
            return sector.GetPlaneTexZ(ceiling: false);

        var spot = sector.Lines[0].V1;
        var floor = double.NegativeInfinity;

        foreach (var check in sector.Lines)
        {
            var other = GetNextSector(check, sector);
            if (other is null)
                continue;

            var ofloor = other.FloorPlane.ZAtPoint(check.V1.X, check.V1.Y);
            if (ofloor > floor)
            {
                floor = ofloor;
                spot = check.V1;
            }

            ofloor = other.FloorPlane.ZAtPoint(check.V2.X, check.V2.Y);
            if (ofloor > floor)
            {
                floor = ofloor;
                spot = check.V2;
            }
        }

        if (spotOut is not null)
        {
            spotOut.X = spot.X;
            spotOut.Y = spot.Y;
        }

        return floor;
    }

    public static double FindNextHighestFloor(MapSector sector, Vertex? spotOut = null)
    {
        if (sector.Lines.Count == 0)
            return sector.GetPlaneTexZ(ceiling: false);

        var spot = sector.Lines[0].V1;
        var height = sector.FloorPlane.ZAtPoint(spot.X, spot.Y);
        var heightdiff = double.MaxValue;

        foreach (var check in sector.Lines)
        {
            var other = GetNextSector(check, sector);
            if (other is null)
                continue;

            var ofloor = other.FloorPlane.ZAtPoint(check.V1.X, check.V1.Y);
            var floor = sector.FloorPlane.ZAtPoint(check.V1.X, check.V1.Y);
            if (ofloor > floor && ofloor - floor < heightdiff && !sector.IsLinked(other, ceiling: false))
            {
                heightdiff = ofloor - floor;
                height = ofloor;
                spot = check.V1;
            }

            ofloor = other.FloorPlane.ZAtPoint(check.V2.X, check.V2.Y);
            floor = sector.FloorPlane.ZAtPoint(check.V2.X, check.V2.Y);
            if (ofloor > floor && ofloor - floor < heightdiff && !sector.IsLinked(other, ceiling: false))
            {
                heightdiff = ofloor - floor;
                height = ofloor;
                spot = check.V2;
            }
        }

        if (spotOut is not null)
        {
            spotOut.X = spot.X;
            spotOut.Y = spot.Y;
        }

        return height;
    }

    public static double FindNextLowestFloor(MapSector sector, Vertex? spotOut = null)
    {
        if (sector.Lines.Count == 0)
            return sector.GetPlaneTexZ(ceiling: false);

        var spot = sector.Lines[0].V1;
        var height = sector.FloorPlane.ZAtPoint(spot.X, spot.Y);
        var heightdiff = double.MaxValue;

        foreach (var check in sector.Lines)
        {
            var other = GetNextSector(check, sector);
            if (other is null)
                continue;

            var ofloor = other.FloorPlane.ZAtPoint(check.V1.X, check.V1.Y);
            var floor = sector.FloorPlane.ZAtPoint(check.V1.X, check.V1.Y);
            if (ofloor < floor && floor - ofloor < heightdiff && !sector.IsLinked(other, ceiling: false))
            {
                heightdiff = floor - ofloor;
                height = ofloor;
                spot = check.V1;
            }

            ofloor = other.FloorPlane.ZAtPoint(check.V2.X, check.V2.Y);
            floor = sector.FloorPlane.ZAtPoint(check.V2.X, check.V2.Y);
            if (ofloor < floor && floor - ofloor < heightdiff && !sector.IsLinked(other, ceiling: false))
            {
                heightdiff = floor - ofloor;
                height = ofloor;
                spot = check.V2;
            }
        }

        if (spotOut is not null)
        {
            spotOut.X = spot.X;
            spotOut.Y = spot.Y;
        }

        return height;
    }

    public static double FindNextLowestCeiling(MapSector sector, Vertex? spotOut = null)
    {
        if (sector.Lines.Count == 0)
            return sector.GetPlaneTexZ(ceiling: false);

        var spot = sector.Lines[0].V1;
        var height = sector.CeilingPlane.ZAtPoint(spot.X, spot.Y);
        var heightdiff = double.MaxValue;

        foreach (var check in sector.Lines)
        {
            var other = GetNextSector(check, sector);
            if (other is null)
                continue;

            var oceil = other.CeilingPlane.ZAtPoint(check.V1.X, check.V1.Y);
            var ceil = sector.CeilingPlane.ZAtPoint(check.V1.X, check.V1.Y);
            if (oceil < ceil && ceil - oceil < heightdiff && !sector.IsLinked(other, ceiling: true))
            {
                heightdiff = ceil - oceil;
                height = oceil;
                spot = check.V1;
            }

            oceil = other.CeilingPlane.ZAtPoint(check.V2.X, check.V2.Y);
            ceil = sector.CeilingPlane.ZAtPoint(check.V2.X, check.V2.Y);
            if (oceil < ceil && ceil - oceil < heightdiff && !sector.IsLinked(other, ceiling: true))
            {
                heightdiff = ceil - oceil;
                height = oceil;
                spot = check.V2;
            }
        }

        if (spotOut is not null)
        {
            spotOut.X = spot.X;
            spotOut.Y = spot.Y;
        }

        return height;
    }

    public static double FindNextHighestCeiling(MapSector sector, Vertex? spotOut = null)
    {
        if (sector.Lines.Count == 0)
            return sector.GetPlaneTexZ(ceiling: true);

        var spot = sector.Lines[0].V1;
        var height = sector.CeilingPlane.ZAtPoint(spot.X, spot.Y);
        var heightdiff = double.MaxValue;

        foreach (var check in sector.Lines)
        {
            var other = GetNextSector(check, sector);
            if (other is null)
                continue;

            var oceil = other.CeilingPlane.ZAtPoint(check.V1.X, check.V1.Y);
            var ceil = sector.CeilingPlane.ZAtPoint(check.V1.X, check.V1.Y);
            if (oceil > ceil && oceil - ceil < heightdiff && !sector.IsLinked(other, ceiling: true))
            {
                heightdiff = oceil - ceil;
                height = oceil;
                spot = check.V1;
            }

            oceil = other.CeilingPlane.ZAtPoint(check.V2.X, check.V2.Y);
            ceil = sector.CeilingPlane.ZAtPoint(check.V2.X, check.V2.Y);
            if (oceil > ceil && oceil - ceil < heightdiff && !sector.IsLinked(other, ceiling: true))
            {
                heightdiff = oceil - ceil;
                height = oceil;
                spot = check.V2;
            }
        }

        if (spotOut is not null)
        {
            spotOut.X = spot.X;
            spotOut.Y = spot.Y;
        }

        return height;
    }
}
