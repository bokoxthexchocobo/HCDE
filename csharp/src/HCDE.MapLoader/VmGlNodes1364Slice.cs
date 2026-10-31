using System.Buffers.Binary;

namespace HCDE.MapLoader;

/// <summary>
/// <c>CheckForMissingSegs</c>, <c>CheckForGLNodes</c>, and <c>LoadGLVertexes</c>
/// from <c>glnodes.cpp</c> (1–240). <c>checkGLVertex</c> at line 241 is not this slice.
/// </summary>
public sealed class VmGlNodes1364Slice
{
    public const int GNd2 = 0x32644E67;
    public const int GNd4 = 0x34644E67;
    public const int GNd5 = 0x35644E67;
    public const int GlVertOffset = 4;
    public const int MapGlVertexSize = 8;
    public const int SeekSet = 0;

    public readonly List<Vertex> Vertexes = new();
    public readonly List<Side> Sides = new();
    public readonly List<Seg> Segs = new();
    public readonly List<Subsector> Subsectors = new();
    public readonly List<LineDef> Lines = new();
    public readonly List<string> Messages = new();
    public int FirstGlVertex;
    public bool Format5;

    public int CheckForMissingSegs()
    {
        var added = new double[Sides.Count];
        foreach (var seg in Segs)
        {
            if (seg.Sidedef == null)
                continue;
            if (seg.V1 == null)
                throw new InvalidOperationException("v1");
            if (seg.V2 == null)
                throw new InvalidOperationException("v2");
            var index = SideIndex(seg.Sidedef);
            var dx = seg.V2.X - seg.V1.X;
            var dy = seg.V2.Y - seg.V1.Y;
            added[index] += Length(dx, dy);
        }

        var missing = 0;
        for (var i = 0; i < Sides.Count; i++)
        {
            var linedef = Sides[i].Linedef;
            if (linedef == null)
                throw new InvalidOperationException("linedef");
            var linelen = Length(linedef.DeltaX, linedef.DeltaY);
            if (added[i] < linelen - 1.0)
                missing++;
        }

        return missing;
    }

    public bool CheckForGLNodes()
    {
        foreach (var sub in Subsectors)
        {
            if (sub.NumLines < 1)
                throw new InvalidOperationException("numlines");
            var last = (long)sub.FirstLine + sub.NumLines - 1L;
            if (sub.FirstLine < 0 || last >= Segs.Count)
                throw new IndexOutOfRangeException("segs");
            var firstSeg = Segs[sub.FirstLine];
            var lastSeg = Segs[(int)last];
            if (!ReferenceEquals(firstSeg.V1, lastSeg.V2))
                return false;
            for (var j = 0; j < sub.NumLines; j++)
            {
                if (Segs[j].Linedef == null)
                    return true;
            }
        }

        var missing = CheckForMissingSegs();
        if (missing > 0)
        {
            Messages.Add(
                missing + " missing segs counted\nThe BSP needs to be rebuilt.\n");
        }

        return missing == 0;
    }

    public bool LoadGlVertexes(GlLump lump)
    {
        FirstGlVertex = Vertexes.Count;
        lump.Seek(0, SeekSet);
        var glbuf = lump.Read();
        var gllen = lump.GetLength();
        if (gllen < 4)
            return false;
        if (glbuf.Length < 4)
            throw new InvalidOperationException("gldata");

        var magic = ReadLittle(glbuf, 0);
        if (magic == GNd5)
            Format5 = true;
        else if (magic != GNd2)
        {
            var version = magic == GNd4 ? 4 : 1;
            Messages.Add(
                "GL nodes v" + version + " found. This format is not supported by HCDE\n");
            return false;
        }
        else
            Format5 = false;

        var added = (gllen - GlVertOffset) / MapGlVertexSize;
        var numvertexes = FirstGlVertex + added;
        if (added < 0 || numvertexes > int.MaxValue)
            throw new InvalidOperationException("numvertexes");
        var count = (int)numvertexes;
        if (Vertexes.Count == 0 && Lines.Count > 0)
            throw new InvalidOperationException("vertexes");

        var old = Vertexes.ToArray();
        while (Vertexes.Count < count)
            Vertexes.Add(new Vertex());

        for (var n = 0; n < Lines.Count; n++)
        {
            var line = Lines[n];
            if (line.V1 == null)
                throw new InvalidOperationException("v1");
            if (line.V2 == null)
                throw new InvalidOperationException("v2");
            var i1 = Array.IndexOf(old, line.V1);
            var i2 = Array.IndexOf(old, line.V2);
            if (i1 < 0 || i2 < 0)
                throw new IndexOutOfRangeException("vertexes");
            line.V1 = Vertexes[i1];
            line.V2 = Vertexes[i2];
        }

        var offset = GlVertOffset;
        for (var i = FirstGlVertex; i < count; i++)
        {
            if (offset > glbuf.Length - MapGlVertexSize)
                throw new IndexOutOfRangeException("gldata");
            var x = ReadLittle(glbuf, offset);
            var y = ReadLittle(glbuf, offset + 4);
            Vertexes[i].X = x / 65536.0;
            Vertexes[i].Y = y / 65536.0;
            offset += MapGlVertexSize;
        }

        return true;
    }

    int SideIndex(Side sidedef)
    {
        for (var i = 0; i < Sides.Count; i++)
        {
            if (ReferenceEquals(Sides[i], sidedef))
                return i;
        }

        throw new IndexOutOfRangeException("sides");
    }

    static double Length(double x, double y)
    {
        return Math.Sqrt((x * x) + (y * y));
    }

    static int ReadLittle(byte[] data, int offset)
    {
        return BinaryPrimitives.ReadInt32LittleEndian(data.AsSpan(offset, 4));
    }

    public sealed class Vertex
    {
        public double X;
        public double Y;
    }

    public sealed class LineDef
    {
        public Vertex? V1;
        public Vertex? V2;
        public double DeltaX;
        public double DeltaY;
    }

    public sealed class Side
    {
        public LineDef? Linedef;
    }

    public sealed class Seg
    {
        public Vertex? V1;
        public Vertex? V2;
        public Side? Sidedef;
        public LineDef? Linedef;
    }

    public sealed class Subsector
    {
        public int FirstLine;
        public int NumLines;
    }

    public sealed class GlLump
    {
        public byte[] Bytes = Array.Empty<byte>();
        public long ReportedLength;
        public int SeekOffset;
        public int SeekOrigin;
        public int SeekCalls;
        public int ReadCalls;
        public int LengthReads;
        public readonly List<string> Trace = new();

        public void Seek(int offset, int origin)
        {
            SeekCalls++;
            SeekOffset = offset;
            SeekOrigin = origin;
            Trace.Add("seek");
        }

        public byte[] Read()
        {
            ReadCalls++;
            Trace.Add("read");
            return Bytes;
        }

        public long GetLength()
        {
            LengthReads++;
            Trace.Add("length");
            return ReportedLength;
        }
    }
}
