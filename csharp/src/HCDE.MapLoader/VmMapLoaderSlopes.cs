namespace HCDE.MapLoader;

/// <summary>Map slope setup from <c>slopes.cpp</c> (41–421).</summary>
public static class VmMapLoaderSlopes
{
    public const double EqualEpsilon = 1.0 / 65536.0;

    public const int SmtSlopeFloorPointLine = 15;
    public const int SmtSlopeCeilingPointLine = 16;
    public const int SmtSetFloorSlope = 17;
    public const int SmtSetCeilingSlope = 18;
    public const int SmtVavoomFloor = 19;
    public const int SmtVavoomCeiling = 20;
    public const int SmtCopyFloorPlane = 21;
    public const int SmtCopyCeilingPlane = 22;
    public const int SmtVertexFloorZ = 23;
    public const int SmtVertexCeilingZ = 24;

    public const uint BcompatfSetSlopeOverflow = 1u << 0;

    public const int VertexFlagZCeilingEnabled = 0x01;
    public const int VertexFlagZFloorEnabled = 0x02;

    public sealed class SecPlane
    {
        public double A { get; private set; }
        public double B { get; private set; }
        public double C { get; private set; }
        public double D { get; private set; }

        public void Set(double a, double b, double c, double dist)
        {
            A = a;
            B = b;
            C = c;
            D = dist;
        }

        public void CopyFrom(SecPlane other) => Set(other.A, other.B, other.C, other.D);

        public double ZAtPoint(double x, double y)
        {
            if (Math.Abs(C) < EqualEpsilon)
                return 0;
            return (D - A * x - B * y) / C;
        }

        public double ZAtVertex(SlopeVertex v) => ZAtPoint(v.X, v.Y);
    }

    public sealed class SlopeVertex
    {
        public double X { get; set; }
        public double Y { get; set; }

        public void Set(double x, double y)
        {
            X = x;
            Y = y;
        }
    }

    public sealed class SlopeLine
    {
        public SlopeVertex V1 { get; } = new();
        public SlopeVertex V2 { get; } = new();
        public SlopeSector? FrontSector { get; set; }
        public SlopeSector? BackSector { get; set; }
        public int[] Args { get; } = new int[5];

        public (double X, double Y) Delta() => (V2.X - V1.X, V2.Y - V1.Y);
    }

    public sealed class SlopeSector
    {
        public List<SlopeLine> Lines { get; } = new();
        public SecPlane FloorPlane { get; } = new();
        public SecPlane CeilingPlane { get; } = new();
        public double FloorTexZ { get; set; }
        public double CeilingTexZ { get; set; }

        public double GetPlaneTexZ(bool ceiling) => ceiling ? CeilingTexZ : FloorTexZ;
    }

    public sealed class MapThingInfo
    {
        public object? Type { get; init; }
        public int Special { get; init; }
    }

    public sealed class SlopeMapThing
    {
        public MapThingInfo? Info { get; set; }
        public double PosX { get; set; }
        public double PosY { get; set; }
        public double PosZ { get; set; }
        public int Angle { get; set; }
        public int ThingId { get; set; }
        public int EdNum { get; set; }
        public int[] Args { get; } = new int[5];
    }

    public sealed class VertexData
    {
        public int Flags { get; set; }
        public double ZFloor { get; set; }
        public double ZCeiling { get; set; }
    }

    public sealed class SlopeTagManager
    {
        public const int TagHashSize = 256;

        readonly List<(int Target, int Tag, int NextTag)> _allIds = new();
        readonly int[] _idHashFirst = new int[TagHashSize];

        public void AddLineId(int line, int tag)
        {
            if (tag == -1)
                return;
            _allIds.Add((line, tag, -1));
        }

        public void AddSectorTag(int sector, int tag)
        {
            if (tag == 0)
                return;
            _sectorTags.Add((sector, tag, -1));
        }

        readonly List<(int Target, int Tag, int NextTag)> _sectorTags = new();
        readonly int[] _tagHashFirst = new int[TagHashSize];

        public void HashTags()
        {
            _allIds.Add((-1, -1, -1));
            Array.Fill(_idHashFirst, -1);
            for (var i = _allIds.Count - 1; i >= 0; i--)
            {
                if (_allIds[i].Target < 0)
                    continue;
                var hash = (int)(((uint)_allIds[i].Tag) % TagHashSize);
                var next = _idHashFirst[hash];
                _allIds[i] = (_allIds[i].Target, _allIds[i].Tag, next);
                _idHashFirst[hash] = i;
            }

            _sectorTags.Add((-1, -1, -1));
            Array.Fill(_tagHashFirst, -1);
            for (var i = _sectorTags.Count - 1; i >= 0; i--)
            {
                if (_sectorTags[i].Target < 0)
                    continue;
                var hash = (int)(((uint)_sectorTags[i].Tag) % TagHashSize);
                var next = _tagHashFirst[hash];
                _sectorTags[i] = (_sectorTags[i].Target, _sectorTags[i].Tag, next);
                _tagHashFirst[hash] = i;
            }
        }

        public int NextLineId(int searchTag, ref int start)
        {
            while (start >= 0 && _allIds[start].Tag != searchTag)
                start = _allIds[start].NextTag;
            if (start < 0)
                return -1;
            var ret = _allIds[start].Target;
            start = _allIds[start].NextTag;
            return ret;
        }

        public int NextSectorTag(int searchTag, ref int start)
        {
            while (start >= 0 && _sectorTags[start].Tag != searchTag)
                start = _sectorTags[start].NextTag;
            if (start < 0)
                return -1;
            var ret = _sectorTags[start].Target;
            start = _sectorTags[start].NextTag;
            return ret;
        }

        public int LineIdHashStart(int searchTag) =>
            _idHashFirst[(int)(((uint)searchTag) % TagHashSize)];

        public int SectorTagHashStart(int searchTag) =>
            _tagHashFirst[(int)(((uint)searchTag) % TagHashSize)];
    }

    public sealed class SlopeLevel
    {
        public List<SlopeLine> Lines { get; } = new();
        public List<SlopeSector> Sectors { get; } = new();
        public List<SlopeVertex> Vertices { get; } = new();
        public List<VertexData> VertexDatas { get; } = new();
        public SlopeTagManager TagManager { get; } = new();
        public uint IBCompatFlags { get; set; }
        public List<string> Log { get; } = new();

        public int Index(SlopeLine line) => Lines.IndexOf(line);

        public int Index(SlopeVertex v)
        {
            for (var i = 0; i < Vertices.Count; i++)
            {
                var vert = Vertices[i];
                if (Math.Abs(vert.X - v.X) < EqualEpsilon && Math.Abs(vert.Y - v.Y) < EqualEpsilon)
                    return i;
            }
            return -1;
        }

        public SlopeSector? PointInSector(double x, double y)
        {
            foreach (var sec in Sectors)
            {
                if (sec.Lines.Count == 0)
                    continue;
                if (PointInSectorInternal(sec, x, y))
                    return sec;
            }
            return null;
        }

        static bool PointInSectorInternal(SlopeSector sec, double x, double y)
        {
            var inside = false;
            foreach (var ln in sec.Lines)
            {
                var (dx, dy) = ln.Delta();
                var cross = (ln.V2.X - ln.V1.X) * (y - ln.V1.Y) - (ln.V2.Y - ln.V1.Y) * (x - ln.V1.X);
                if (cross > EqualEpsilon)
                    inside = !inside;
            }
            return inside;
        }
    }

    public static int PointOnLineSidePrecise(double x, double y, SlopeLine line)
    {
        var (dx, dy) = line.Delta();
        return (y - line.V1.Y) * dx + (line.V1.X - x) * dy > EqualEpsilon ? 1 : 0;
    }

    static (double X, double Y, double Z) Cross((double X, double Y, double Z) a, (double X, double Y, double Z) b) =>
        (a.Y * b.Z - a.Z * b.Y, a.Z * b.X - a.X * b.Z, a.X * b.Y - a.Y * b.X);

    static double Length((double X, double Y, double Z) v) =>
        Math.Sqrt(v.X * v.X + v.Y * v.Y + v.Z * v.Z);

    static (double X, double Y, double Z) Unit((double X, double Y, double Z) v)
    {
        var len = Length(v);
        return len == 0 ? (0, 0, 0) : (v.X / len, v.Y / len, v.Z / len);
    }

    public static void SlopeLineToPoint(SlopeLevel level, int lineId, double posX, double posY, double posZ, bool slopeCeil)
    {
        var start = level.TagManager.LineIdHashStart(lineId);
        while (true)
        {
            var linenum = level.TagManager.NextLineId(lineId, ref start);
            if (linenum < 0)
                break;
            var line = level.Lines[linenum];
            SlopeSector? sec;
            if (PointOnLineSidePrecise(posX, posY, line) == 0)
                sec = line.FrontSector;
            else
                sec = line.BackSector;
            if (sec is null)
                continue;

            var plane = slopeCeil ? sec.CeilingPlane : sec.FloorPlane;
            var pZ = plane.ZAtVertex(line.V1);
            var (dX, dY) = line.Delta();
            var v1 = (dX, dY, plane.ZAtVertex(line.V2) - pZ);
            var v2 = (posX - line.V1.X, posY - line.V1.Y, posZ - pZ);
            var cross = Cross(v1, v2);
            var len = Length(cross);
            if (len == 0)
            {
                level.Log.Add($"Slope thing at ({posX},{posY}) lies directly on its target line.");
                return;
            }
            cross = (cross.X / len, cross.Y / len, cross.Z / len);
            if ((cross.Z < 0 && !slopeCeil) || (cross.Z > 0 && slopeCeil))
                cross = (-cross.X, -cross.Y, -cross.Z);

            var dist = -cross.X * posX - cross.Y * posY - cross.Z * posZ;
            plane.Set(cross.X, cross.Y, cross.Z, dist);
        }
    }

    public static void CopyPlane(SlopeLevel level, int tag, SlopeSector dest, bool copyCeil)
    {
        var start = level.TagManager.SectorTagHashStart(tag);
        var secnum = level.TagManager.NextSectorTag(tag, ref start);
        if (secnum < 0)
            return;
        var source = level.Sectors[secnum];
        if (copyCeil)
            dest.CeilingPlane.CopyFrom(source.CeilingPlane);
        else
            dest.FloorPlane.CopyFrom(source.FloorPlane);
    }

    public static void CopyPlane(SlopeLevel level, int tag, double posX, double posY, bool copyCeil)
    {
        var dest = level.PointInSector(posX, posY);
        if (dest is null)
            return;
        CopyPlane(level, tag, dest, copyCeil);
    }

    public static void SetSlope(
        SlopeLevel level,
        SecPlane plane,
        bool setCeil,
        int xyangi,
        int zangi,
        double posX,
        double posY,
        double posZ)
    {
        double zangDeg;
        if (zangi >= 180)
            zangDeg = 179;
        else if (zangi <= 0)
            zangDeg = 1;
        else
            zangDeg = zangi;
        if (setCeil)
            zangDeg += 180;

        var xyangRad = xyangi * (Math.PI / 180.0);
        var zangRad = zangDeg * (Math.PI / 180.0);

        double normX, normY;
        if ((level.IBCompatFlags & BcompatfSetSlopeOverflow) != 0)
        {
            normX = FloatToFixed(Math.Cos(zangRad)) * FloatToFixed(Math.Cos(xyangRad)) / 65536.0;
            normY = FloatToFixed(Math.Cos(zangRad)) * FloatToFixed(Math.Sin(xyangRad)) / 65536.0;
        }
        else
        {
            normX = Math.Cos(zangRad) * Math.Cos(xyangRad);
            normY = Math.Cos(zangRad) * Math.Sin(xyangRad);
        }

        var normZ = Math.Sin(zangRad);
        var (nx, ny, nz) = Unit((normX, normY, normZ));
        var dist = -nx * posX - ny * posY - nz * posZ;
        plane.Set(nx, ny, nz, dist);
    }

    static int FloatToFixed(double v) => (int)(v * 65536.0);

    public static void VavoomSlope(
        SlopeLevel level,
        SlopeSector sec,
        int id,
        double posX,
        double posY,
        double posZ,
        bool ceiling)
    {
        foreach (var l in sec.Lines)
        {
            if (l.Args[0] != id)
                continue;
            var srcplane = ceiling ? sec.CeilingPlane : sec.FloorPlane;
            var srcheight = sec.GetPlaneTexZ(ceiling);
            var v1 = (posX - l.V2.X, posY - l.V2.Y, posZ - srcheight);
            var v2 = (posX - l.V1.X, posY - l.V1.Y, posZ - srcheight);
            var cross = Cross(v1, v2);
            var len = Length(cross);
            if (len == 0)
            {
                level.Log.Add($"Slope thing at ({posX},{posY}) lies directly on its target line.");
                return;
            }
            cross = (cross.X / len, cross.Y / len, cross.Z / len);
            var which = ceiling ? 1 : 0;
            if ((cross.Z < 0 && which == 0) || (cross.Z > 0 && which == 1))
                cross = (-cross.X, -cross.Y, -cross.Z);
            var dist = -cross.X * posX - cross.Y * posY - cross.Z * posZ;
            srcplane.Set(cross.X, cross.Y, cross.Z, dist);
            return;
        }
    }

    public static void SetSlopesFromVertexHeights(
        SlopeLevel level,
        IList<SlopeMapThing> things,
        int firstIndex,
        int lastIndex,
        int[]? oldVertexTable)
    {
        var vtHeightsFloor = new Dictionary<int, double>();
        var vtHeightsCeil = new Dictionary<int, double>();
        var vtFound = false;

        for (var ti = firstIndex; ti < lastIndex; ti++)
        {
            var mt = things[ti];
            if (mt.Info?.Type is not null)
                continue;
            if (mt.Info is null)
                continue;
            if (mt.Info.Special == SmtVertexFloorZ || mt.Info.Special == SmtVertexCeilingZ)
            {
                for (var i = 0; i < level.Vertices.Count; i++)
                {
                    var v = level.Vertices[i];
                    if (v.X == mt.PosX && v.Y == mt.PosY)
                    {
                        if (mt.Info.Special == SmtVertexFloorZ)
                            vtHeightsFloor[i] = mt.PosZ;
                        else
                            vtHeightsCeil[i] = mt.PosZ;
                        vtFound = true;
                    }
                }
                mt.EdNum = 0;
            }
        }

        for (var i = 0; i < level.VertexDatas.Count; i++)
        {
            var ii = oldVertexTable is null ? i : oldVertexTable[i];
            var data = level.VertexDatas[i];
            if ((data.Flags & VertexFlagZCeilingEnabled) != 0)
            {
                vtHeightsCeil[ii] = data.ZCeiling;
                vtFound = true;
            }
            if ((data.Flags & VertexFlagZFloorEnabled) != 0)
            {
                vtHeightsFloor[ii] = data.ZFloor;
                vtFound = true;
            }
        }

        level.VertexDatas.Clear();

        if (!vtFound)
            return;

        foreach (var sec in level.Sectors)
        {
            if (sec.Lines.Count != 3)
                continue;

            var ln0 = sec.Lines[0];
            var vi1 = level.Index(ln0.V1);
            var vi2 = level.Index(ln0.V2);
            var l1v1 = level.Index(sec.Lines[1].V1);
            var l1v2 = level.Index(sec.Lines[1].V2);
            var vi3 = l1v1 == vi1 || l1v1 == vi2 ? l1v2 : l1v1;

            var v1 = level.Vertices[vi1];
            var v2 = level.Vertices[vi2];
            var v3 = level.Vertices[vi3];

            for (var j = 0; j < 2; j++)
            {
                var heightMap = j == 0 ? vtHeightsFloor : vtHeightsCeil;
                var has1 = heightMap.ContainsKey(vi1);
                var has2 = heightMap.ContainsKey(vi2);
                var has3 = heightMap.ContainsKey(vi3);
                if (!has1 && !has2 && !has3)
                    continue;

                var z1 = has1 ? heightMap[vi1] : sec.GetPlaneTexZ(j == 1);
                var z2 = has2 ? heightMap[vi2] : sec.GetPlaneTexZ(j == 1);
                var z3 = has3 ? heightMap[vi3] : sec.GetPlaneTexZ(j == 1);

                (double X, double Y, double Z) vec1, vec2;
                if (PointOnLineSidePrecise(v3.X, v3.Y, ln0) == 0)
                {
                    vec1 = (v2.X - v3.X, v2.Y - v3.Y, z2 - z3);
                    vec2 = (v1.X - v3.X, v1.Y - v3.Y, z1 - z3);
                }
                else
                {
                    vec1 = (v1.X - v3.X, v1.Y - v3.Y, z1 - z3);
                    vec2 = (v2.X - v3.X, v2.Y - v3.Y, z2 - z3);
                }

                var cross = Cross(vec1, vec2);
                var len = Length(cross);
                if (len == 0)
                    continue;
                cross = (cross.X / len, cross.Y / len, cross.Z / len);
                if ((cross.Z < 0 && j == 0) || (cross.Z > 0 && j == 1))
                    cross = (-cross.X, -cross.Y, -cross.Z);

                var plane = j == 0 ? sec.FloorPlane : sec.CeilingPlane;
                var dist = -cross.X * v3.X - cross.Y * v3.Y - cross.Z * z3;
                plane.Set(cross.X, cross.Y, cross.Z, dist);
            }
        }
    }

    public static void SpawnSlopeMakers(
        SlopeLevel level,
        IList<SlopeMapThing> things,
        int firstIndex,
        int lastIndex,
        int[]? oldVertexTable)
    {
        for (var ti = firstIndex; ti < lastIndex; ti++)
        {
            var mt = things[ti];
            if (mt.Info?.Type is not null)
                continue;
            if (mt.Info is null)
                continue;
            var special = mt.Info.Special;
            if (special < SmtSlopeFloorPointLine || special > SmtVavoomCeiling)
                continue;

            var sec = level.PointInSector(mt.PosX, mt.PosY);
            if (sec is null)
                continue;

            var ceiling = special == SmtSlopeCeilingPointLine || special == SmtVavoomCeiling || special == SmtSetCeilingSlope;
            var plane = ceiling ? sec.CeilingPlane : sec.FloorPlane;
            var posZ = plane.ZAtPoint(mt.PosX, mt.PosY) + mt.PosZ;

            if (special <= SmtSlopeCeilingPointLine)
                SlopeLineToPoint(level, mt.Args[0], mt.PosX, mt.PosY, posZ, ceiling);
            else if (special <= SmtSetCeilingSlope)
                SetSlope(level, plane, ceiling, mt.Angle, mt.Args[0], mt.PosX, mt.PosY, posZ);
            else
                VavoomSlope(level, sec, mt.ThingId, mt.PosX, mt.PosY, mt.PosZ, ceiling);
            mt.EdNum = 0;
        }

        SetSlopesFromVertexHeights(level, things, firstIndex, lastIndex, oldVertexTable);

        for (var ti = firstIndex; ti < lastIndex; ti++)
        {
            var mt = things[ti];
            if (mt.Info?.Type is not null)
                continue;
            if (mt.Info is null)
                continue;
            if (mt.Info.Special == SmtCopyFloorPlane || mt.Info.Special == SmtCopyCeilingPlane)
            {
                CopyPlane(level, mt.Args[0], mt.PosX, mt.PosY, mt.Info.Special == SmtCopyCeilingPlane);
                mt.EdNum = 0;
            }
        }
    }
}
