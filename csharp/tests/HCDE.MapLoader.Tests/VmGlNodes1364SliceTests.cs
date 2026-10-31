using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.MapLoader.Tests;

public class VmGlNodes1364SliceTests
{
    [Fact]
    public void MissingSegsAndClosedSubsectors()
    {
        var exact = new VmGlNodes1364Slice();
        var side = Side(5);
        exact.Sides.Add(side);
        exact.Segs.Add(Cover(side, 4));
        Assert.Equal(0, exact.CheckForMissingSegs());
        Assert.Empty(exact.Messages);

        var shortSide = new VmGlNodes1364Slice();
        shortSide.Sides.Add(Side(5));
        shortSide.Segs.Add(Cover(shortSide.Sides[0], 3));
        Assert.Equal(1, shortSide.CheckForMissingSegs());

        var stored = new VmGlNodes1364Slice();
        var wide = Side(5);
        var wideLine = wide.Linedef;
        Assert.NotNull(wideLine);
        wideLine.V1 = new VmGlNodes1364Slice.Vertex();
        wideLine.V2 = new VmGlNodes1364Slice.Vertex { X = 100 };
        stored.Sides.Add(wide);
        stored.Segs.Add(Cover(wide, 4));
        Assert.Equal(0, stored.CheckForMissingSegs());

        var over = new VmGlNodes1364Slice();
        over.Sides.Add(Side(5));
        over.Segs.Add(Cover(over.Sides[0], 9));
        Assert.Equal(0, over.CheckForMissingSegs());

        var zero = new VmGlNodes1364Slice();
        var zeroSide = Side(0);
        zero.Sides.Add(zeroSide);
        Assert.Equal(0, zero.CheckForMissingSegs());
        var zeroLine = zeroSide.Linedef;
        Assert.NotNull(zeroLine);
        zeroLine.DeltaX = 1;
        Assert.Equal(0, zero.CheckForMissingSegs());
        zeroLine.DeltaX = 1.5;
        Assert.Equal(1, zero.CheckForMissingSegs());

        var diagonal = new VmGlNodes1364Slice();
        var diagSide = Side(7);
        diagonal.Sides.Add(diagSide);
        diagonal.Segs.Add(new VmGlNodes1364Slice.Seg
        {
            V1 = new VmGlNodes1364Slice.Vertex(),
            V2 = new VmGlNodes1364Slice.Vertex { X = 3, Y = 4 },
            Sidedef = diagSide,
            Linedef = diagSide.Linedef,
        });
        Assert.Equal(1, diagonal.CheckForMissingSegs());
        var diagLine = diagSide.Linedef;
        Assert.NotNull(diagLine);
        diagLine.DeltaX = 6;
        Assert.Equal(0, diagonal.CheckForMissingSegs());

        var both = new VmGlNodes1364Slice();
        both.Sides.Add(Side(10));
        both.Sides.Add(Side(4));
        both.Segs.Add(Cover(both.Sides[1], 4));
        Assert.Equal(1, both.CheckForMissingSegs());

        var pile = new VmGlNodes1364Slice();
        pile.Sides.Add(Side(8));
        pile.Segs.Add(Cover(pile.Sides[0], 3));
        Assert.Equal(1, pile.CheckForMissingSegs());
        pile.Segs.Add(Cover(pile.Sides[0], 4));
        Assert.Equal(0, pile.CheckForMissingSegs());

        var skipped = new VmGlNodes1364Slice();
        skipped.Sides.Add(Side(0));
        skipped.Segs.Add(new VmGlNodes1364Slice.Seg { V1 = null, V2 = null });
        Assert.Equal(0, skipped.CheckForMissingSegs());

        var bare = new VmGlNodes1364Slice();
        var orphan = new VmGlNodes1364Slice.Side();
        bare.Segs.Add(new VmGlNodes1364Slice.Seg
        {
            V1 = new VmGlNodes1364Slice.Vertex(),
            V2 = new VmGlNodes1364Slice.Vertex { X = 1 },
            Sidedef = orphan,
        });
        var missingSide = Assert.Throws<IndexOutOfRangeException>(() => bare.CheckForMissingSegs());
        Assert.Equal("sides", missingSide.Message);

        var broken = new VmGlNodes1364Slice();
        var hosted = new VmGlNodes1364Slice.Side { Linedef = new VmGlNodes1364Slice.LineDef() };
        broken.Sides.Add(hosted);
        broken.Segs.Add(new VmGlNodes1364Slice.Seg { Sidedef = hosted });
        var missingVertex = Assert.Throws<InvalidOperationException>(() => broken.CheckForMissingSegs());
        Assert.Equal("v1", missingVertex.Message);

        var noLine = new VmGlNodes1364Slice();
        noLine.Sides.Add(new VmGlNodes1364Slice.Side());
        var missingLine = Assert.Throws<InvalidOperationException>(() => noLine.CheckForMissingSegs());
        Assert.Equal("linedef", missingLine.Message);
        Assert.Empty(noLine.Messages);

        var open = new VmGlNodes1364Slice();
        open.Sides.Add(new VmGlNodes1364Slice.Side());
        var left = new VmGlNodes1364Slice.Vertex();
        var right = new VmGlNodes1364Slice.Vertex();
        open.Segs.Add(new VmGlNodes1364Slice.Seg
        {
            V1 = left,
            V2 = right,
            Linedef = new VmGlNodes1364Slice.LineDef(),
        });
        open.Subsectors.Add(new VmGlNodes1364Slice.Subsector { FirstLine = 0, NumLines = 1 });
        Assert.False(open.CheckForGLNodes());
        Assert.Empty(open.Messages);

        var early = new VmGlNodes1364Slice();
        early.Sides.Add(new VmGlNodes1364Slice.Side());
        var loop = new VmGlNodes1364Slice.Vertex();
        early.Segs.Add(new VmGlNodes1364Slice.Seg { V1 = loop, V2 = loop });
        early.Subsectors.Add(new VmGlNodes1364Slice.Subsector { FirstLine = 0, NumLines = 1 });
        Assert.True(early.CheckForGLNodes());
        Assert.Empty(early.Messages);

        var global = new VmGlNodes1364Slice();
        global.Sides.Add(new VmGlNodes1364Slice.Side());
        var shared = new VmGlNodes1364Slice.Vertex();
        global.Segs.Add(new VmGlNodes1364Slice.Seg { V1 = shared, V2 = shared });
        global.Segs.Add(new VmGlNodes1364Slice.Seg
        {
            V1 = shared,
            V2 = shared,
            Linedef = new VmGlNodes1364Slice.LineDef(),
        });
        global.Subsectors.Add(new VmGlNodes1364Slice.Subsector { FirstLine = 1, NumLines = 1 });
        Assert.True(global.CheckForGLNodes());

        var hidden = new VmGlNodes1364Slice();
        var covered = Side(4);
        hidden.Sides.Add(covered);
        var hub = new VmGlNodes1364Slice.Vertex();
        hidden.Segs.Add(new VmGlNodes1364Slice.Seg
        {
            V1 = hub,
            V2 = hub,
            Linedef = covered.Linedef,
        });
        hidden.Segs.Add(Cover(covered, 4));
        hidden.Segs[1].V1 = hub;
        hidden.Segs[1].V2 = hub;
        hidden.Segs[1].Linedef = null;
        hidden.Subsectors.Add(new VmGlNodes1364Slice.Subsector { FirstLine = 0, NumLines = 1 });
        Assert.False(hidden.CheckForGLNodes());
        Assert.Equal(
            "1 missing segs counted\nThe BSP needs to be rebuilt.\n",
            Assert.Single(hidden.Messages));

        var twins = new VmGlNodes1364Slice();
        twins.Segs.Add(new VmGlNodes1364Slice.Seg
        {
            V1 = new VmGlNodes1364Slice.Vertex(),
            V2 = new VmGlNodes1364Slice.Vertex(),
        });
        twins.Subsectors.Add(new VmGlNodes1364Slice.Subsector { FirstLine = 0, NumLines = 1 });
        Assert.False(twins.CheckForGLNodes());

        var blanks = new VmGlNodes1364Slice();
        blanks.Segs.Add(new VmGlNodes1364Slice.Seg
        {
            Linedef = new VmGlNodes1364Slice.LineDef(),
        });
        blanks.Subsectors.Add(new VmGlNodes1364Slice.Subsector { FirstLine = 0, NumLines = 1 });
        Assert.True(blanks.CheckForGLNodes());
        Assert.Empty(blanks.Messages);

        var none = new VmGlNodes1364Slice();
        Assert.True(none.CheckForGLNodes());
        var gap = Side(3);
        none.Sides.Add(gap);
        none.Sides.Add(Side(3));
        Assert.False(none.CheckForGLNodes());
        Assert.Equal(
            "2 missing segs counted\nThe BSP needs to be rebuilt.\n",
            Assert.Single(none.Messages));

        var emptyWindow = new VmGlNodes1364Slice();
        emptyWindow.Segs.Add(new VmGlNodes1364Slice.Seg());
        emptyWindow.Subsectors.Add(new VmGlNodes1364Slice.Subsector { NumLines = 0 });
        var badCount = Assert.Throws<InvalidOperationException>(() => emptyWindow.CheckForGLNodes());
        Assert.Equal("numlines", badCount.Message);

        var past = new VmGlNodes1364Slice();
        past.Segs.Add(new VmGlNodes1364Slice.Seg());
        past.Subsectors.Add(new VmGlNodes1364Slice.Subsector { FirstLine = 0, NumLines = 3 });
        var pastEnd = Assert.Throws<IndexOutOfRangeException>(() => past.CheckForGLNodes());
        Assert.Equal("segs", pastEnd.Message);
        past.Subsectors[0].FirstLine = -1;
        past.Subsectors[0].NumLines = 1;
        var negative = Assert.Throws<IndexOutOfRangeException>(() => past.CheckForGLNodes());
        Assert.Equal("segs", negative.Message);
    }

    [Fact]
    public void LoadGlVertexes()
    {
        Assert.Equal(8, VmGlNodes1364Slice.MapGlVertexSize);
        Assert.Equal(4, VmGlNodes1364Slice.GlVertOffset);
        Assert.Equal(0, VmGlNodes1364Slice.SeekSet);
        Assert.Equal(VmGlNodes1364Slice.GNd2, Magic("gNd2"));
        Assert.Equal(VmGlNodes1364Slice.GNd4, Magic("gNd4"));
        Assert.Equal(VmGlNodes1364Slice.GNd5, Magic("gNd5"));

        var slice = new VmGlNodes1364Slice { Format5 = true };
        var shortLump = Raw(new byte[] { (byte)'g', (byte)'N', (byte)'d', (byte)'2' }, 3);
        Assert.False(slice.LoadGlVertexes(shortLump));
        Assert.True(slice.Format5);
        Assert.Equal(0, slice.FirstGlVertex);
        Assert.Empty(slice.Vertexes);
        Assert.Empty(slice.Messages);
        Assert.Equal(new[] { "seek", "read", "length" }, shortLump.Trace);
        Assert.Equal(0, shortLump.SeekOffset);
        Assert.Equal(VmGlNodes1364Slice.SeekSet, shortLump.SeekOrigin);
        Assert.Equal(1, shortLump.SeekCalls);
        Assert.Equal(1, shortLump.ReadCalls);
        Assert.Equal(1, shortLump.LengthReads);

        var negative = Raw(Array.Empty<byte>(), -1);
        Assert.False(slice.LoadGlVertexes(negative));
        Assert.True(slice.Format5);

        var kept = new VmGlNodes1364Slice.Vertex { X = 3, Y = 4 };
        slice.Vertexes.Add(kept);
        var rejected = Lump("gNd4");
        Assert.False(slice.LoadGlVertexes(rejected));
        Assert.True(slice.Format5);
        Assert.Equal(1, slice.FirstGlVertex);
        Assert.Same(kept, Assert.Single(slice.Vertexes));
        Assert.Equal(
            "GL nodes v4 found. This format is not supported by HCDE\n",
            Assert.Single(slice.Messages));

        slice.Messages.Clear();
        var unknown = Raw(new byte[] { 1, 2, 3, 4 }, 4);
        Assert.False(slice.LoadGlVertexes(unknown));
        Assert.Equal(
            "GL nodes v1 found. This format is not supported by HCDE\n",
            Assert.Single(slice.Messages));
        Assert.True(slice.Format5);

        slice.Messages.Clear();
        var swapped = Raw(new byte[] { 0x32, 0x64, 0x4E, 0x67 }, 4);
        Assert.False(slice.LoadGlVertexes(swapped));
        Assert.Equal(
            "GL nodes v1 found. This format is not supported by HCDE\n",
            Assert.Single(slice.Messages));

        var fresh = new VmGlNodes1364Slice();
        var gnd5 = Lump("gNd5", (65536, -65536));
        Assert.True(fresh.LoadGlVertexes(gnd5));
        Assert.True(fresh.Format5);
        Assert.Equal(0, fresh.FirstGlVertex);
        var loaded = Assert.Single(fresh.Vertexes);
        Assert.Equal(1.0, loaded.X);
        Assert.Equal(-1.0, loaded.Y);

        var host = new VmGlNodes1364Slice { Format5 = true };
        var old = new VmGlNodes1364Slice.Vertex { X = 9, Y = 8 };
        var other = new VmGlNodes1364Slice.Vertex { X = 2, Y = 3 };
        host.Vertexes.Add(old);
        host.Vertexes.Add(other);
        var line = new VmGlNodes1364Slice.LineDef { V1 = old, V2 = other };
        var second = new VmGlNodes1364Slice.LineDef { V1 = other, V2 = old };
        host.Lines.Add(line);
        host.Lines.Add(second);
        var gnd2 = Lump("gNd2", 3, (32768, 1));
        Assert.True(host.LoadGlVertexes(gnd2));
        Assert.False(host.Format5);
        Assert.Equal(2, host.FirstGlVertex);
        Assert.Equal(3, host.Vertexes.Count);
        Assert.Same(old, host.Vertexes[0]);
        Assert.Same(other, host.Vertexes[1]);
        Assert.Equal(9, host.Vertexes[0].X);
        Assert.Equal(8, host.Vertexes[0].Y);
        Assert.Same(old, line.V1);
        Assert.Same(other, line.V2);
        Assert.Same(other, second.V1);
        Assert.Same(old, second.V2);
        Assert.Equal(0.5, host.Vertexes[2].X);
        Assert.Equal(1.0 / 65536.0, host.Vertexes[2].Y);
        Assert.Empty(host.Messages);

        var endian = new VmGlNodes1364Slice();
        var big = new List<byte> { (byte)'g', (byte)'N', (byte)'d', (byte)'2' };
        var be = new byte[4];
        BinaryPrimitives.WriteInt32BigEndian(be, 65536);
        big.AddRange(be);
        big.AddRange(new byte[4]);
        var bigLump = Raw(big.ToArray(), big.Count);
        Assert.True(endian.LoadGlVertexes(bigLump));
        Assert.Equal(256.0 / 65536.0, Assert.Single(endian.Vertexes).X);
        Assert.Equal(0, endian.Vertexes[0].Y);

        var same = Lump("gNd5", (65536, 0));
        var leftSlice = new VmGlNodes1364Slice();
        var rightSlice = new VmGlNodes1364Slice { Format5 = true };
        Assert.True(leftSlice.LoadGlVertexes(same));
        Assert.True(rightSlice.LoadGlVertexes(Lump("gNd2", (65536, 0))));
        Assert.True(leftSlice.Format5);
        Assert.False(rightSlice.Format5);
        Assert.Equal(leftSlice.Vertexes[0].X, rightSlice.Vertexes[0].X);
        Assert.Equal(leftSlice.Vertexes[0].Y, rightSlice.Vertexes[0].Y);

        var empty = new VmGlNodes1364Slice { Format5 = true };
        Assert.True(empty.LoadGlVertexes(Lump("gNd2")));
        Assert.False(empty.Format5);
        Assert.Empty(empty.Vertexes);

        var blocked = new VmGlNodes1364Slice();
        blocked.Lines.Add(new VmGlNodes1364Slice.LineDef
        {
            V1 = new VmGlNodes1364Slice.Vertex { X = 4 },
        });
        var blockedLump = Lump("gNd5", (65536, 0));
        var noBase = Assert.Throws<InvalidOperationException>(() => blocked.LoadGlVertexes(blockedLump));
        Assert.Equal("vertexes", noBase.Message);
        Assert.True(blocked.Format5);
        Assert.Empty(blocked.Vertexes);
        Assert.Equal(4, blocked.Lines[0].V1!.X);

        var clipped = new VmGlNodes1364Slice();
        var anchor = new VmGlNodes1364Slice.Vertex { X = 3, Y = 4 };
        clipped.Vertexes.Add(anchor);
        clipped.Lines.Add(new VmGlNodes1364Slice.LineDef { V1 = anchor, V2 = anchor });
        var shortBuf = Lump("gNd5", (65536, 131072));
        shortBuf.ReportedLength = 4 + 16;
        var overrun = Assert.Throws<IndexOutOfRangeException>(() => clipped.LoadGlVertexes(shortBuf));
        Assert.Equal("gldata", overrun.Message);
        Assert.True(clipped.Format5);
        Assert.Equal(1, clipped.FirstGlVertex);
        Assert.Equal(3, clipped.Vertexes.Count);
        Assert.Equal(3, clipped.Vertexes[0].X);
        Assert.Equal(1, clipped.Vertexes[1].X);
        Assert.Equal(2, clipped.Vertexes[1].Y);
        Assert.Equal(0, clipped.Vertexes[2].X);
        Assert.Equal(0, clipped.Vertexes[2].Y);
        Assert.Same(anchor, clipped.Lines[0].V1);

        var torn = new VmGlNodes1364Slice();
        torn.Vertexes.Add(new VmGlNodes1364Slice.Vertex());
        var foreign = new VmGlNodes1364Slice.Vertex { X = 9 };
        torn.Lines.Add(new VmGlNodes1364Slice.LineDef
        {
            V1 = foreign,
            V2 = torn.Vertexes[0],
        });
        var tornLump = Lump("gNd2", (65536, 0));
        var wild = Assert.Throws<IndexOutOfRangeException>(() => torn.LoadGlVertexes(tornLump));
        Assert.Equal("vertexes", wild.Message);
        Assert.False(torn.Format5);
        Assert.Equal(2, torn.Vertexes.Count);
        Assert.Equal(0, torn.Vertexes[1].X);
        Assert.Same(foreign, torn.Lines[0].V1);

        var half = new VmGlNodes1364Slice();
        var real = new VmGlNodes1364Slice.Vertex();
        half.Vertexes.Add(real);
        half.Lines.Add(new VmGlNodes1364Slice.LineDef { V1 = real, V2 = null });
        var missingEnd = Assert.Throws<InvalidOperationException>(() => half.LoadGlVertexes(Lump("gNd2")));
        Assert.Equal("v2", missingEnd.Message);
        Assert.Empty(half.Messages);

        var header = new VmGlNodes1364Slice { Format5 = true };
        var tiny = Raw(new byte[] { 1, 2, 3 }, 8);
        var badMagic = Assert.Throws<InvalidOperationException>(() => header.LoadGlVertexes(tiny));
        Assert.Equal("gldata", badMagic.Message);
        Assert.True(header.Format5);
        Assert.Empty(header.Vertexes);
        Assert.Empty(header.Messages);

        var huge = new VmGlNodes1364Slice();
        var hugeLump = Lump("gNd5");
        hugeLump.ReportedLength = 4L + (8L * ((long)int.MaxValue + 1L));
        var overflow = Assert.Throws<InvalidOperationException>(() => huge.LoadGlVertexes(hugeLump));
        Assert.Equal("numvertexes", overflow.Message);
        Assert.True(huge.Format5);
        Assert.Empty(huge.Vertexes);
        Assert.Equal(new[] { "seek", "read", "length" }, hugeLump.Trace);
    }

    static int Magic(string text)
    {
        var bytes = new byte[4];
        for (var i = 0; i < 4; i++)
            bytes[i] = (byte)text[i];
        return BinaryPrimitives.ReadInt32LittleEndian(bytes);
    }

    static VmGlNodes1364Slice.GlLump Raw(byte[] bytes, long length)
    {
        return new VmGlNodes1364Slice.GlLump
        {
            Bytes = bytes,
            ReportedLength = length,
        };
    }

    static VmGlNodes1364Slice.GlLump Lump(string magic, params (int X, int Y)[] verts)
    {
        return Lump(magic, 0, verts);
    }

    static VmGlNodes1364Slice.GlLump Lump(string magic, int extra, params (int X, int Y)[] verts)
    {
        var bytes = new List<byte>();
        for (var i = 0; i < 4; i++)
            bytes.Add((byte)magic[i]);
        foreach (var vert in verts)
        {
            var raw = new byte[8];
            BinaryPrimitives.WriteInt32LittleEndian(raw, vert.X);
            BinaryPrimitives.WriteInt32LittleEndian(raw.AsSpan(4), vert.Y);
            bytes.AddRange(raw);
        }

        for (var i = 0; i < extra; i++)
            bytes.Add(0xAB);
        return Raw(bytes.ToArray(), bytes.Count);
    }

    static VmGlNodes1364Slice.Side Side(double delta)
    {
        return new VmGlNodes1364Slice.Side
        {
            Linedef = new VmGlNodes1364Slice.LineDef { DeltaX = delta },
        };
    }

    static VmGlNodes1364Slice.Seg Cover(VmGlNodes1364Slice.Side side, double length)
    {
        return new VmGlNodes1364Slice.Seg
        {
            V1 = new VmGlNodes1364Slice.Vertex(),
            V2 = new VmGlNodes1364Slice.Vertex { X = length },
            Sidedef = side,
            Linedef = side.Linedef,
        };
    }
}
