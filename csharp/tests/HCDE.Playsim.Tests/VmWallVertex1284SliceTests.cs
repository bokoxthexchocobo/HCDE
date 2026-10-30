using System.Globalization;
using HCDE.Playsim;

namespace HCDE.Playsim.Tests;

public class VmWallVertex1284SliceTests
{
    static string Dump(VmWallVertexBuild1284 wall)
    {
        return string.Join('\n', wall.Vertices.Select(v =>
            string.Join(' ', new[] { v.X, v.Z, v.Y, v.U, v.V, v.Lu, v.Lv, v.LIndex }
                .Select(n => n.ToString("G9", CultureInfo.InvariantCulture)))));
    }

    static VmWallVertexBuild1284 Sample()
    {
        var wall = new VmWallVertexBuild1284
        {
            X1 = 1,
            Y1 = 2,
            X2 = 9,
            Y2 = 4,
            FracLeft = 0,
            FracRight = 1,
            LIndex = 7,
            ReserveCursor = 40,
            Side = new VmWallSide1284
            {
                NumSegs = 2,
                Segs =
                {
                    new VmWallSeg1284 { SideFrac = 0.5f, X2 = 5, Y2 = 6 },
                    new VmWallSeg1284 { SideFrac = 0.75f, X2 = 8, Y2 = 9 },
                },
            },
            LeftHeights = new VmWallHeight1284 { NumHeights = 3, Heights = new[] { 0f, 2f, 4f } },
            RightHeights = new VmWallHeight1284 { NumHeights = 3, Heights = new[] { 2f, 4f, 8f } },
        };
        wall.ZTop[0] = 4;
        wall.ZTop[1] = 8;
        wall.ZCeil[0] = 100;
        wall.Tex[VmWallVertexBuild1284.UpLeft].U = 2;
        wall.Tex[VmWallVertexBuild1284.UpLeft].V = 1;
        wall.Tex[VmWallVertexBuild1284.UpRight].U = 6;
        wall.Tex[VmWallVertexBuild1284.UpRight].V = 3;
        wall.Tex[VmWallVertexBuild1284.LoRight].U = 8;
        wall.Tex[VmWallVertexBuild1284.LoRight].V = 4;
        wall.Light[VmWallVertexBuild1284.UpLeft].U = 10;
        wall.Light[VmWallVertexBuild1284.UpLeft].V = 20;
        wall.Light[VmWallVertexBuild1284.UpRight].U = 8;
        wall.Light[VmWallVertexBuild1284.UpRight].V = 8;
        wall.Light[VmWallVertexBuild1284.LoRight].U = 4;
        wall.Light[VmWallVertexBuild1284.LoRight].V = 8;
        return wall;
    }

    [Fact]
    public void Create_InsertsTheOpenHeightsAndSkipsTheRightZeroSlot()
    {
        var wall = Sample();
        wall.MakeVertices(false);
        Assert.Equal(8, wall.Allocated);
        Assert.Equal(8, wall.VertCount);
        Assert.Equal(40, wall.VertIndex);
        Assert.Equal(
            "1 0 2 0 0 0 0 7\n" +
            "1 2 2 1 0.5 5 10 7\n" +
            "1 4 2 2 1 10 20 7\n" +
            "5 6 6 4 2 9 14 7\n" +
            "9 8 4 6 3 8 8 7\n" +
            "9 4 4 7 3.5 6 8 7\n" +
            "9 0 4 8 4 4 8 7\n" +
            "5 0 6 4 2 2 4 7",
            Dump(wall));
        Assert.DoesNotContain("100", Dump(wall));
        int index = wall.VertIndex;
        wall.MakeVertices(false);
        Assert.Equal(index, wall.VertIndex);
        Assert.Equal(8, wall.VertCount);
    }

    [Fact]
    public void Count_OverAllocatesOutsideSplitsAndWrapsAnEmptySide()
    {
        var ordered = new VmWallVertexBuild1284
        {
            X1 = 1,
            X2 = 9,
            FracLeft = 0,
            FracRight = 1,
            Side = new VmWallSide1284
            {
                NumSegs = 3,
                Segs =
                {
                    new VmWallSeg1284 { SideFrac = 0.25f, X2 = 11 },
                    new VmWallSeg1284 { SideFrac = 0.5f, X2 = 22 },
                    new VmWallSeg1284 { SideFrac = 0.9f, X2 = 33 },
                },
            },
        };
        ordered.MakeVertices(false);
        Assert.Equal(8, ordered.Allocated);
        Assert.Equal(new[] { 1f, 1f, 11f, 22f, 9f, 9f, 22f, 11f }, ordered.Vertices.Select(v => v.X).ToArray());

        var outside = Sample();
        outside.Vertices.Clear();
        outside.VertCount = 0;
        outside.FracLeft = 0.2f;
        outside.FracRight = 0.8f;
        outside.LeftHeights = null;
        outside.RightHeights = null;
        outside.Side!.NumSegs = 3;
        outside.Side.Segs.Clear();
        outside.Side.Segs.Add(new VmWallSeg1284 { SideFrac = 0.1f, X2 = 11 });
        outside.Side.Segs.Add(new VmWallSeg1284 { SideFrac = 0.1f, X2 = 22 });
        outside.Side.Segs.Add(new VmWallSeg1284 { SideFrac = 0.1f, X2 = 33 });
        outside.MakeVertices(false);
        Assert.Equal(8, outside.Allocated);
        Assert.Equal(4, outside.VertCount);

        var empty = new VmWallVertexBuild1284
        {
            FracLeft = 0.5f,
            FracRight = 0.5f,
            Side = new VmWallSide1284 { NumSegs = 0 },
        };
        Assert.Equal(2, empty.CountVertices());

        var plain = new VmWallVertexBuild1284 { Seamless = false, Side = new VmWallSide1284 { NumSegs = 4 } };
        plain.MakeVertices(false);
        Assert.Equal(4, plain.Allocated);
        plain = new VmWallVertexBuild1284 { Side = null };
        plain.MakeVertices(false);
        Assert.Equal(4, plain.Allocated);
        plain = new VmWallVertexBuild1284 { Flags = VmWallVertexBuild1284.NoSplit, Side = new VmWallSide1284() };
        plain.MakeVertices(false);
        Assert.Equal(4, plain.Allocated);
        plain = new VmWallVertexBuild1284
        {
            Side = new VmWallSide1284 { Flags = VmWallVertexBuild1284.WallPoly, NumSegs = 4 },
        };
        plain.MakeVertices(false);
        Assert.Equal(4, plain.Allocated);

        var upperStop = new VmWallVertexBuild1284
        {
            FracLeft = 0.2f,
            FracRight = 0.8f,
            Flags = VmWallVertexBuild1284.NoSplitLower,
            Side = new VmWallSide1284
            {
                NumSegs = 3,
                Segs =
                {
                    new VmWallSeg1284 { SideFrac = 0.9f, X2 = 11 },
                    new VmWallSeg1284 { SideFrac = 0.5f, X2 = 22 },
                    new VmWallSeg1284 { SideFrac = 0.4f, X2 = 33 },
                },
            },
        };
        upperStop.MakeVertices(false);
        Assert.DoesNotContain(upperStop.Vertices, v => v.X == 22f);

        var lowerStop = new VmWallVertexBuild1284
        {
            FracLeft = 0.2f,
            FracRight = 0.8f,
            Flags = VmWallVertexBuild1284.NoSplitUpper,
            Side = new VmWallSide1284
            {
                NumSegs = 3,
                Segs =
                {
                    new VmWallSeg1284 { SideFrac = 0.5f, X2 = 11 },
                    new VmWallSeg1284 { SideFrac = 0.9f, X2 = 22 },
                    new VmWallSeg1284 { SideFrac = 0.4f, X2 = 33 },
                },
            },
        };
        lowerStop.MakeVertices(false);
        Assert.Contains(lowerStop.Vertices, v => v.X == 11f);
        Assert.DoesNotContain(lowerStop.Vertices, v => v.X == 22f);
    }
}
