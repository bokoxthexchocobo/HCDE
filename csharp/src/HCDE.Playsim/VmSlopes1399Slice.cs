namespace HCDE.Playsim;

/// <summary>
/// Sector slope alignment and copy from <c>slopes.cpp</c> 422-588. Chunk 1399.
/// <c>CopyPlane</c> and <c>GetPlaneTexZ</c> stay callees. The file ends at line 588.
/// </summary>
public static class VmSlopes1399Slice
{
    public const int PlaneAlign = 181;
    public const int PlaneCopy = 118;
    public const int BandStart = 422;
    public const int BandEnd = 588;

    public sealed class Vertex
    {
        public double X;
        public double Y;
    }

    public sealed class SectorEdge
    {
        public Vertex? V1;
        public Vertex? V2;
    }

    public sealed class Plane
    {
        public double X;
        public double Y;
        public double Z;
        public double D;
        public double NegiC;

        public void Set(double a, double b, double c, double d)
        {
            X = a;
            Y = b;
            Z = c;
            D = d;
            NegiC = -1.0 / c;
        }

        public void CopyFrom(Plane other)
        {
            if (other == null)
                throw new ArgumentNullException(nameof(other));
            X = other.X;
            Y = other.Y;
            Z = other.Z;
            D = other.D;
            NegiC = other.NegiC;
        }
    }

    public sealed class Sector
    {
        public List<SectorEdge> Lines { get; } = new();
        public double FloorTexZ;
        public double CeilingTexZ;
        public Plane Floor { get; } = new();
        public Plane Ceiling { get; } = new();
    }

    public sealed class MapLine
    {
        public int Special;
        public int[] Args { get; } = new int[5];
        public Vertex? V1;
        public double DeltaX;
        public double DeltaY;
        public Sector? Front;
        public Sector? Back;
    }

    public readonly record struct Vec3(double X, double Y, double Z);

    public static void AlignPlane(Sector sector, MapLine line, int which)
    {
        if (sector == null)
            throw new ArgumentNullException(nameof(sector));
        if (line == null)
            throw new ArgumentNullException(nameof(line));
        Vertex first = sector.Lines.Count == 0
            ? throw new InvalidOperationException("lines")
            : sector.Lines[0].V1 ?? throw new InvalidOperationException("lines");
        if (line.Back == null)
            return;
        if (line.V1 == null)
            throw new ArgumentNullException(nameof(line));

        Vertex refVert = first;
        double best = 0;
        foreach (SectorEdge edge in sector.Lines)
        {
            Vertex left = edge.V1 ?? throw new InvalidOperationException("vertex");
            Vertex right = edge.V2 ?? throw new InvalidOperationException("vertex");
            for (int i = 0; i < 2; i++)
            {
                Vertex vert = i == 0 ? left : right;
                double dist = Math.Abs((line.V1.Y - vert.Y) * line.DeltaX - (line.V1.X - vert.X) * line.DeltaY);
                if (dist > best)
                {
                    best = dist;
                    refVert = vert;
                }
            }
        }

        Sector? other = ReferenceEquals(line.Front, sector) ? line.Back : line.Front;
        if (other == null)
            throw new InvalidOperationException("reference");
        Sector reference = other;
        bool floor = which == 0;
        double sourceHeight = floor ? sector.FloorTexZ : sector.CeilingTexZ;
        double destHeight = floor ? reference.FloorTexZ : reference.CeilingTexZ;
        var v1 = new Vec3(line.DeltaX, line.DeltaY, 0);
        var v2 = new Vec3(refVert.X - line.V1.X, refVert.Y - line.V1.Y, sourceHeight - destHeight);
        Vec3 cross = Unit(Cross(v1, v2));
        if ((cross.Z < 0 && which == 0) || (cross.Z > 0 && which == 1))
            cross = new Vec3(-cross.X, -cross.Y, -cross.Z);
        double planeD = -cross.X * line.V1.X - cross.Y * line.V1.Y - cross.Z * destHeight;
        Plane plane = floor ? sector.Floor : sector.Ceiling;
        plane.Set(cross.X, cross.Y, cross.Z, planeD);
    }

    public static void SetSlopes(IList<MapLine> lines)
    {
        if (lines == null)
            throw new ArgumentNullException(nameof(lines));
        foreach (MapLine line in lines)
        {
            if (line.Special != PlaneAlign)
                continue;
            line.Special = 0;
            if (line.Back == null)
                continue;
            for (int s = 0; s < 2; s++)
            {
                int bits = line.Args[s] & 3;
                if (s == 1 && bits == 0)
                    bits = (line.Args[0] >> 2) & 3;
                if (bits == 1)
                    AlignPlane(line.Front ?? throw new InvalidOperationException("front"), line, s);
                else if (bits == 2)
                    AlignPlane(line.Back, line, s);
            }
        }
    }

    public static void CopySlopes(IList<MapLine> lines, Action<int, Sector, bool> copyPlane)
    {
        if (lines == null)
            throw new ArgumentNullException(nameof(lines));
        if (copyPlane == null)
            throw new ArgumentNullException(nameof(copyPlane));
        foreach (MapLine line in lines)
        {
            if (line.Special != PlaneCopy)
                continue;
            line.Special = 0;
            int limit = line.Back != null ? 4 : 2;
            for (int s = 0; s < limit; s++)
            {
                if (line.Args[s] == 0)
                    continue;
                Sector dest = (s & 2) != 0
                    ? line.Back ?? throw new InvalidOperationException("back")
                    : line.Front ?? throw new InvalidOperationException("front");
                copyPlane(line.Args[s], dest, (s & 1) != 0);
            }
            if (line.Back == null)
                continue;
            int side = line.Args[4] & 3;
            if (side == 1)
                line.Back.Floor.CopyFrom((line.Front ?? throw new InvalidOperationException("front")).Floor);
            else if (side == 2)
                (line.Front ?? throw new InvalidOperationException("front")).Floor.CopyFrom(line.Back.Floor);
            int vertical = line.Args[4] & 12;
            if (vertical == 4)
                line.Back.Ceiling.CopyFrom((line.Front ?? throw new InvalidOperationException("front")).Ceiling);
            else if (vertical == 8)
                (line.Front ?? throw new InvalidOperationException("front")).Ceiling.CopyFrom(line.Back.Ceiling);
        }
    }

    public static Vec3 Cross(Vec3 a, Vec3 b)
    {
        return new Vec3(a.Y * b.Z - a.Z * b.Y, a.Z * b.X - a.X * b.Z, a.X * b.Y - a.Y * b.X);
    }

    public static Vec3 Unit(Vec3 value)
    {
        double len = Math.Sqrt(value.X * value.X + value.Y * value.Y + value.Z * value.Z);
        if (len != 0)
            len = 1 / len;
        return new Vec3(value.X * len, value.Y * len, value.Z * len);
    }
}
