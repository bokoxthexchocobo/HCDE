namespace HCDE.Playsim.Tests;

public class VmDecal1273SliceTests
{
    [Fact]
    public void Heights_UseThePegAndTheMidSector()
    {
        var open = Line(false);
        var pass = View();
        pass.RenderDecals(Chain(
            Decal(1, VmDecalPass1273.Invisible),
            Decal(2, 0, d => d.PicValid = false),
            Decal(3, 0, d => d.PicPresent = false),
            Decal(4, 0, d => d.ProjectReject = true),
            Decal(5, 0, d => { d.Sx1 = 10; d.Sx2 = 12; }),
            Decal(6, 0, d => { d.Sx1 = -2; d.Sx2 = 0; }),
            Decal(7, 0, d => { d.Sx1 = 4; d.Sx2 = 8; }),
            Decal(8, 0, d => { d.Sx1 = 3; d.Sx2 = 4; }),
            Decal(9, 0),
            Decal(10, VmDecalPass1273.RelUpper),
            Decal(11, VmDecalPass1273.RelLower),
            Decal(12, VmDecalPass1273.RelMid),
            Decal(13, VmDecalPass1273.RelUpper | VmDecalPass1273.ClipMid),
            Decal(14, VmDecalPass1273.RelMid | VmDecalPass1273.ClipMid)), open, 0, 4, false);
        Assert.Equal(
"""
8 z 3 drawn 11 tier 1 map base flip 0 0 bright 0 proj 3 4 clip 0 4 floor ceiling ceil walltop edge 5.3999999999999995 13.199999999999999 15 26 style 1 alpha 0.333333343 trans 9 color 11
8 z 3 drawn 11 tier 1 map base flip 0 0 bright 0 proj 3 4 clip 0 4 floor wallbottom ceil floor edge 5.3999999999999995 13.199999999999999 15 26 style 1 alpha 0.333333343 trans 9 color 11
9 z 3 drawn 11 tier 1 map base flip 0 0 bright 0 proj 1 3 clip 0 4 floor ceiling ceil walltop edge 5.3999999999999995 13.199999999999999 15 26 style 1 alpha 0.333333343 trans 9 color 11
9 z 3 drawn 11 tier 1 map base flip 0 0 bright 0 proj 1 3 clip 0 4 floor wallbottom ceil floor edge 5.3999999999999995 13.199999999999999 15 26 style 1 alpha 0.333333343 trans 9 color 11
10 z 83 drawn 91 tier 1 map base flip 0 0 bright 0 proj 1 3 clip 0 4 floor ceiling ceil walltop edge 5.3999999999999995 13.199999999999999 15 26 style 1 alpha 0.333333343 trans 9 color 11
10 z 83 drawn 91 tier 1 map base flip 0 0 bright 0 proj 1 3 clip 0 4 floor wallbottom ceil floor edge 5.3999999999999995 13.199999999999999 15 26 style 1 alpha 0.333333343 trans 9 color 11
11 z 8 drawn 16 tier 1 map base flip 0 0 bright 0 proj 1 3 clip 0 4 floor ceiling ceil walltop edge 5.3999999999999995 13.199999999999999 15 26 style 1 alpha 0.333333343 trans 9 color 11
11 z 8 drawn 16 tier 1 map base flip 0 0 bright 0 proj 1 3 clip 0 4 floor wallbottom ceil floor edge 5.3999999999999995 13.199999999999999 15 26 style 1 alpha 0.333333343 trans 9 color 11
12 z 103 drawn 111 tier 1 map base flip 0 0 bright 0 proj 1 3 clip 0 4 floor ceiling ceil walltop edge 5.3999999999999995 13.199999999999999 15 26 style 1 alpha 0.333333343 trans 9 color 11
12 z 103 drawn 111 tier 1 map base flip 0 0 bright 0 proj 1 3 clip 0 4 floor wallbottom ceil floor edge 5.3999999999999995 13.199999999999999 15 26 style 1 alpha 0.333333343 trans 9 color 11
""",
            string.Join('\n', pass.Log));

        var peg = Line(true);
        var pegged = View();
        pegged.RenderDecals(Chain(
            Decal(20, VmDecalPass1273.RelUpper),
            Decal(21, VmDecalPass1273.RelLower),
            Decal(22, VmDecalPass1273.RelMid),
            Decal(23, VmDecalPass1273.RelUpper | VmDecalPass1273.ClipMid),
            Decal(24, VmDecalPass1273.RelMid | VmDecalPass1273.ClipMid)), peg, 0, 4, false);
        Assert.Equal(
"""
20 z 103 drawn 111 tier 1 map base flip 0 0 bright 0 proj 1 3 clip 0 4 floor ceiling ceil walltop edge 5.3999999999999995 13.199999999999999 15 26 style 1 alpha 0.333333343 trans 9 color 11
20 z 103 drawn 111 tier 1 map base flip 0 0 bright 0 proj 1 3 clip 0 4 floor wallbottom ceil floor edge 5.3999999999999995 13.199999999999999 15 26 style 1 alpha 0.333333343 trans 9 color 11
21 z 103 drawn 111 tier 1 map base flip 0 0 bright 0 proj 1 3 clip 0 4 floor ceiling ceil walltop edge 5.3999999999999995 13.199999999999999 15 26 style 1 alpha 0.333333343 trans 9 color 11
21 z 103 drawn 111 tier 1 map base flip 0 0 bright 0 proj 1 3 clip 0 4 floor wallbottom ceil floor edge 5.3999999999999995 13.199999999999999 15 26 style 1 alpha 0.333333343 trans 9 color 11
22 z 13 drawn 21 tier 1 map base flip 0 0 bright 0 proj 1 3 clip 0 4 floor ceiling ceil walltop edge 5.3999999999999995 13.199999999999999 15 26 style 1 alpha 0.333333343 trans 9 color 11
22 z 13 drawn 21 tier 1 map base flip 0 0 bright 0 proj 1 3 clip 0 4 floor wallbottom ceil floor edge 5.3999999999999995 13.199999999999999 15 26 style 1 alpha 0.333333343 trans 9 color 11
""",
            string.Join('\n', pegged.Log));

        var sided = View();
        var one = Line(false);
        one.HasBack = false;
        one.BackCeiling = 999;
        one.BackFloor = 999;
        sided.RenderDecals(Decal(30, VmDecalPass1273.RelUpper), one, 0, 4, false);
        sided.RenderDecals(Decal(31, VmDecalPass1273.RelLower), one, 0, 4, false);
        var zero = new VmDecalLine1273 { HasBack = false, FrontCeiling = 100, FrontFloor = 10, V1X = 4, V1Y = 5, V2X = 4, V2Y = 5 };
        sided.RenderDecals(Decal(32, 0, d => { d.X = 8; d.Y = 9; d.PicWidth = 0; d.LeftOffset = 0; }), zero, 0, 4, false);
        Assert.Equal(
"""
30 z 103 drawn 111 tier 1 map base flip 0 0 bright 0 proj 1 3 clip 0 4 floor wallbottom ceil walltop edge 5.3999999999999995 13.199999999999999 15 26 style 1 alpha 0.333333343 trans 9 color 11
31 z 13 drawn 21 tier 1 map base flip 0 0 bright 0 proj 1 3 clip 0 4 floor wallbottom ceil walltop edge 5.3999999999999995 13.199999999999999 15 26 style 1 alpha 0.333333343 trans 9 color 11
32 z 3 drawn 11 tier 1 map base flip 0 0 bright 0 proj 1 3 clip 0 4 floor wallbottom ceil walltop edge 7 7 7 7 style 1 alpha 0.333333343 trans 9 color 11
""",
            string.Join('\n', sided.Log));

        var mid = View();
        mid.RenderDecals(Decal(60, VmDecalPass1273.RelUpper | VmDecalPass1273.ClipMid), open, 0, 4, true);
        mid.RenderDecals(Decal(61, VmDecalPass1273.RelUpper | VmDecalPass1273.ClipMid), peg, 0, 4, true);
        mid.RenderDecals(Decal(62, VmDecalPass1273.RelMid | VmDecalPass1273.ClipMid), peg, 0, 4, true);
        Assert.Equal(
"""
60 z 83 drawn 91 tier 1 map base flip 0 0 bright 0 proj 1 3 clip 0 4 floor wallbottom ceil walltop edge 5.3999999999999995 13.199999999999999 15 26 style 1 alpha 0.333333343 trans 9 color 11
61 z 43 drawn 51 tier 1 map base flip 0 0 bright 0 proj 1 3 clip 0 4 floor wallbottom ceil walltop edge 5.3999999999999995 13.199999999999999 15 26 style 1 alpha 0.333333343 trans 9 color 11
62 z 10 drawn 18 tier 1 map base flip 0 0 bright 0 proj 1 3 clip 0 4 floor wallbottom ceil walltop edge 5.3999999999999995 13.199999999999999 15 26 style 1 alpha 0.333333343 trans 9 color 11
""",
            string.Join('\n', mid.Log));

        var quiet = View();
        quiet.ViewActive = false;
        quiet.RenderDecals(Decal(19, 0), open, 0, 4, false);
        Assert.Empty(quiet.Log);
        var dead = View();
        dead.ThreadPresent = false;
        dead.RenderDecals(Decal(33, 0, d => d.PicPresent = false), open, 0, 4, false);
        Assert.Empty(dead.Log);
        Assert.Throws<NullReferenceException>(() => dead.RenderDecals(Decal(34, 0), open, 0, 4, false));
        Assert.Throws<NullReferenceException>(() => pass.RenderDecals(Decal(35, 0), null, 0, 4, false));
    }

    [Fact]
    public void Clips_RepeatOnlyTheTwoSidedFullWall()
    {
        var open = Line(false);
        var clips = View();
        clips.ColormapColor = 9;
        clips.ColormapFade = 5;
        clips.ColormapDesaturate = 2;
        clips.RenderDecals(Chain(
            Mark(40, VmDecalPass1273.ClipUpper),
            Mark(41, VmDecalPass1273.ClipFull | VmDecalPass1273.XFlip | VmDecalPass1273.YFlip | VmDecalPass1273.FullBright, d => d.Style = VmDecalPass1273.StyleAdd),
            Mark(42, VmDecalPass1273.ClipMid)), open, 0, 4, true);
        Assert.Equal(
"""
41 z 10 drawn 18 tier 1 map special 9 0 2 flip 1 1 bright 1 proj 1 3 clip 0 4 floor wallbottom ceil walltop edge 5.3999999999999995 13.199999999999999 15 26 style 7 alpha 0.333333343 trans 9 color 11
42 z 10 drawn 18 tier 1 map base flip 0 0 bright 0 proj 1 3 clip 0 4 floor wallbottom ceil walltop edge 5.3999999999999995 13.199999999999999 15 26 style 1 alpha 0.333333343 trans 9 color 11
""",
            string.Join('\n', clips.Log));
        Assert.DoesNotContain("sprtop", string.Join('\n', clips.Log));
        Assert.DoesNotContain("sprbottom", string.Join('\n', clips.Log));

        clips.Log.Clear();
        clips.RenderDecals(Chain(
            Mark(43, VmDecalPass1273.ClipMid),
            Mark(44, VmDecalPass1273.ClipFull, d => d.Visible = false),
            Mark(45, VmDecalPass1273.ClipUpper),
            Mark(46, VmDecalPass1273.ClipLower),
            Mark(47, VmDecalPass1273.ClipFull, d => d.Style = VmDecalPass1273.StyleAdd)), open, 0, 4, false);
        Assert.Equal(
"""
45 z 10 drawn 18 tier 0 map base flip 0 0 bright 0 proj 1 3 clip 0 4 floor ceiling ceil walltop edge 5.3999999999999995 13.199999999999999 15 26 style 1 alpha 0.333333343 trans 9 color 11
46 z 10 drawn 18 tier 2 map base flip 0 0 bright 0 proj 1 3 clip 0 4 floor wallbottom ceil floor edge 5.3999999999999995 13.199999999999999 15 26 style 1 alpha 0.333333343 trans 9 color 11
47 z 10 drawn 18 tier 1 map special 9 0 2 flip 0 0 bright 0 proj 1 3 clip 0 4 floor ceiling ceil walltop edge 5.3999999999999995 13.199999999999999 15 26 style 7 alpha 0.333333343 trans 9 color 11
47 z 10 drawn 18 tier 1 map special 9 0 2 flip 0 0 bright 0 proj 1 3 clip 0 4 floor wallbottom ceil floor edge 5.3999999999999995 13.199999999999999 15 26 style 7 alpha 0.333333343 trans 9 color 11
""",
            string.Join('\n', clips.Log));

        var plain = View();
        plain.ColormapColor = 9;
        plain.ColormapDesaturate = 2;
        var one = Line(false);
        one.HasBack = false;
        plain.RenderDecals(Mark(48, 0, d => d.Style = VmDecalPass1273.StyleAdd), one, 0, 4, false);
        plain.ColormapFade = 5;
        plain.RenderDecals(Mark(49, 0, d => d.Style = 1), one, 0, 4, false);
        plain.RenderDecals(Mark(50, VmDecalPass1273.ClipFull), one, 0, 4, false);
        Assert.Equal(
"""
48 z 10 drawn 18 tier 1 map base flip 0 0 bright 0 proj 1 3 clip 0 4 floor wallbottom ceil walltop edge 5.3999999999999995 13.199999999999999 15 26 style 7 alpha 0.333333343 trans 9 color 11
49 z 10 drawn 18 tier 1 map base flip 0 0 bright 0 proj 1 3 clip 0 4 floor wallbottom ceil walltop edge 5.3999999999999995 13.199999999999999 15 26 style 1 alpha 0.333333343 trans 9 color 11
50 z 10 drawn 18 tier 1 map base flip 0 0 bright 0 proj 1 3 clip 0 4 floor wallbottom ceil walltop edge 5.3999999999999995 13.199999999999999 15 26 style 1 alpha 0.333333343 trans 9 color 11
""",
            string.Join('\n', plain.Log));
    }

    static VmDecalLine1273 Line(bool peg) => new()
    {
        HasBack = true,
        FrontCeiling = 100,
        FrontFloor = 10,
        BackCeiling = 80,
        BackFloor = 5,
        LineFlags = peg ? VmDecalPass1273.DontPegTop | VmDecalPass1273.DontPegBottom : 0,
        V1X = 0,
        V1Y = 0,
        V2X = 3,
        V2Y = 4,
    };

    static VmDecalPass1273 View() => new() { ViewX = 1, ViewY = 2 };

    static VmDecal1273 Decal(int id, int flags, Action<VmDecal1273>? tune = null)
    {
        var d = new VmDecal1273
        {
            Id = id,
            RenderFlags = flags,
            Z = 3,
            PicWidth = 8,
            LeftOffset = 3,
            TopOffset = 4,
            ScaleX = 2,
            ScaleY = 2,
            X = 10,
            Y = 20,
            Sx1 = 1,
            Sx2 = 3,
            Style = 1,
            Alpha = 1.0 / 3.0,
            Translation = 9,
            AlphaColor = 11,
            SectorCeiling = 40,
            SectorFloor = 7,
        };
        tune?.Invoke(d);
        return d;
    }

    static VmDecal1273 Mark(int id, int flags, Action<VmDecal1273>? tune = null)
    {
        var d = Decal(id, flags, tune);
        d.Z = 10;
        return d;
    }

    static VmDecal1273 Chain(params VmDecal1273[] decals)
    {
        for (int i = 0; i < decals.Length - 1; i++)
            decals[i].WallNext = decals[i + 1];
        return decals[0];
    }
}
