using S = HCDE.Playsim.VmSaveSidePlane1388Slice;

namespace HCDE.Playsim.Tests;

public class VmSaveSidePlane1388SliceTests
{
    [Fact]
    public void ManipulationPartAndSideKeepMismatchedKeys()
    {
        var changed = new S.Manip { Add = 0x11, Mod = 0x22, Blend = 0x33, Desat = 0.25f };
        var fx = new S.Arc();
        S.Texture(fx, "fx", changed, new S.Manip());
        Assert.Equal(
            "obj:fx\nf:addcolor:00000011|00000000\nf:yoffset:00000022|00000000\nf:xscale:00000033|00000000\nf:yscale:0.25|0\nend",
            fx.Text());
        Assert.DoesNotContain("modulate", fx.Text());
        Assert.DoesNotContain("blendcolor", fx.Text());

        var same = new S.Arc();
        S.Texture(same, "fx", new S.Manip(), new S.Manip());
        Assert.Equal("", same.Text());

        var rolled = new S.Arc { Rollback = true };
        S.Texture(rolled, "fx", new S.Manip(), new S.Manip());
        Assert.Contains("obj:fx", rolled.Text());
        Assert.Contains("f:yscale:0|0", rolled.Text());

        var top = new S.Arc { RootObject = false };
        S.Texture(top, "fx", new S.Manip(), new S.Manip());
        Assert.Contains("obj:fx", top.Text());

        var part = new S.Part { Interp = new S.Ptr("i"), Add = 5, Fx = new S.Manip { Add = 9 } };
        var partArc = new S.Arc();
        S.WallPart(partArc, "part", part, new S.Part());
        Assert.Contains("f:interpolation:i\n", partArc.Text());
        Assert.Contains("f:addcolor:00000005|00000000\nobj:texturefx\nf:addcolor:00000009|00000000", partArc.Text());
        Assert.DoesNotContain("f:texturefx:", partArc.Text());

        var side = new S.Side { Light = 4, Decals = new S.Ptr("d") };
        var sideArc = new S.Arc();
        S.Wall(sideArc, "side", side, new S.Side { Light = 4 });
        Assert.Equal("obj:side\nf:light:4|4\nf:flags:0|0\nf:attacheddecals:d\nend", sideArc.Text());
        Assert.DoesNotContain("leftside", sideArc.Text());
        Assert.DoesNotContain("rightside", sideArc.Text());
        Assert.DoesNotContain("index", sideArc.Text());

        var mixed = new S.Side();
        mixed.Tex[1].X = 3;
        var mixedArc = new S.Arc();
        S.Wall(mixedArc, "side", mixed, new S.Side());
        var lines = mixedArc.Text().Split('\n');
        Assert.Equal(3, lines.Count(line => line == "obj"));
        Assert.Contains("arr:textures", mixedArc.Text());
        Assert.Contains("f:xoffset:3|0", mixedArc.Text());
        Assert.Contains("f:xoffset:0|0", mixedArc.Text());
        Assert.DoesNotContain("obj:texturefx", mixedArc.Text());

        var again = new S.Side();
        var rollSide = new S.Arc { Rollback = true };
        S.Wall(rollSide, "side", again, new S.Side());
        Assert.Contains("arr:textures", rollSide.Text());

        var read = new S.Arc { Writing = false, ReadCount = 0 };
        S.Wall(read, "side", new S.Side(), new S.Side());
        Assert.Equal("obj:side\narr:textures\nendarr\nf:light:0|0\nf:flags:0|0\nf:attacheddecals:null\nend", read.Text());

        var one = new S.Arc { Writing = false, ReadCount = 1 };
        S.Wall(one, "side", new S.Side(), new S.Side());
        Assert.Single(one.Text().Split('\n'), line => line == "obj");
        Assert.Contains("obj:texturefx", one.Text());

        var closed = new S.Arc { Writing = false, OpenObject = false };
        S.Wall(closed, "side", new S.Side(), null);
        Assert.Equal("", closed.Text());

        var bare = new S.Arc();
        var thrown = Assert.Throws<NullReferenceException>(() => S.Wall(bare, "side", new S.Side(), null));
        Assert.NotNull(thrown);
        Assert.Equal("obj:side", bare.Text());

        var nameless = new S.Arc();
        var missing = Assert.Throws<InvalidOperationException>(() => S.WallPart(nameless, null, new S.Part { X = 1 }, new S.Part()));
        Assert.Equal("missing element name", missing.Message);
        Assert.Equal("", nameless.Text());

        var link = new S.Arc();
        S.Linked(link, "link", new S.Link { Sector = new S.Ptr("s"), Type = 2 }, new S.Link { Type = 9 });
        Assert.Equal("obj:link\nf:sector:s\nf:type:2\nend", link.Text());

        var tint = new S.Map { Blend = 1 };
        var tintArc = new S.Arc();
        S.Colormap(tintArc, "cm", tint, new S.Map());
        Assert.Equal("obj:cm\nf:lightcolor:00FFFFFF\nf:fadecolor:00000000\nf:desaturation:0\nf:fogdensity:0\nend", tintArc.Text());
        Assert.DoesNotContain("blend", tintArc.Text());
        var quiet = new S.Arc();
        S.Colormap(quiet, "cm", new S.Map(), new S.Map());
        Assert.Equal("", quiet.Text());

        var line = new S.Arc();
        Assert.Same(line, S.CloseLine(line));
        Assert.Equal("return", line.Text());
    }

    [Fact]
    public void SectorPlaneSkipsOnBytesAndSecplaneRepairsNegiC()
    {
        var sky = new S.Plane { Sky0 = 7, Fx = new S.Manip { Desat = 0.5f } };
        var skyArc = new S.Arc();
        S.SectorPlane(skyArc, "floor", sky, new S.Plane());
        Assert.Contains("f:xoffs:0|0", skyArc.Text());
        Assert.Contains("f:baseyoffs:0|0", skyArc.Text());
        Assert.Contains("f:baseangle:0|0", skyArc.Text());
        Assert.Contains("obj:texturefx", skyArc.Text());
        Assert.Contains("f:yscale:0.5|0", skyArc.Text());
        Assert.DoesNotContain("sky", skyArc.Text());

        var left = new S.Plane();
        left.Xf.Y = 1;
        left.Xf.BaseY = 2;
        var right = new S.Plane();
        right.Xf.Y = 3;
        right.Xf.BaseY = 0;
        var sum = new S.Arc();
        S.SectorPlane(sum, "floor", left, right);
        Assert.Contains("f:yoffs:1|3", sum.Text());
        Assert.Contains("f:baseyoffs:2|0", sum.Text());

        var quiet = new S.Arc();
        S.SectorPlane(quiet, "floor", new S.Plane(), new S.Plane());
        Assert.Equal("", quiet.Text());

        var moved = new S.Sec { NegiC = 5 };
        var still = new S.Arc();
        S.Secplane(still, "sec", moved, new S.Sec { NegiC = 9 });
        Assert.Equal(5.0, moved.NegiC);
        Assert.Contains("obj:sec", still.Text());
        Assert.Contains("f:d:0|0", still.Text());
        Assert.DoesNotContain("arr:", still.Text());

        var dither = new S.Sec { Dither = true };
        var ditherArc = new S.Arc();
        S.Secplane(ditherArc, "sec", dither, new S.Sec());
        Assert.Contains("obj:sec", ditherArc.Text());

        var both = new S.Arc();
        S.Secplane(both, "sec", new S.Sec { NegiC = 1.5, Dither = true }, new S.Sec { NegiC = 1.5, Dither = true });
        Assert.Equal("", both.Text());

        var read = new S.Sec { NegiC = 9, D = 1 };
        var readArc = new S.Arc { Writing = false, ReadZ = 2, ReadD = 4 };
        S.Secplane(readArc, "sec", read, null);
        Assert.Equal(2.0, read.Z);
        Assert.Equal(4.0, read.D);
        Assert.Equal(-0.5, read.NegiC);
        Assert.Contains("f64:0,0,2|null", readArc.Text());
        Assert.Contains("f:d:4|null", readArc.Text());

        var zero = new S.Sec { Z = 4, NegiC = 9 };
        S.Secplane(new S.Arc { Writing = false, ReadZ = 0 }, "sec", zero, null);
        Assert.Equal(0.0, zero.Z);
        Assert.Equal(9.0, zero.NegiC);

        var closed = new S.Sec { Z = 4, NegiC = 9 };
        var closedArc = new S.Arc { Writing = false, OpenObject = false, ReadZ = 2 };
        S.Secplane(closedArc, "sec", closed, null);
        Assert.Equal(4.0, closed.Z);
        Assert.Equal(9.0, closed.NegiC);
        Assert.Equal("", closedArc.Text());

        var kept = new S.Sec { Z = 2, NegiC = 9 };
        var keptArc = new S.Arc { Writing = false, OpenArray = false, ReadZ = 8 };
        S.Secplane(keptArc, "sec", kept, null);
        Assert.Equal(2.0, kept.Z);
        Assert.Equal(-0.5, kept.NegiC);
        Assert.DoesNotContain("arr:", keptArc.Text());

        var down = new S.Sec { NegiC = 9 };
        S.Secplane(new S.Arc { Writing = false, ReadZ = -4 }, "sec", down, null);
        Assert.Equal(0.25, down.NegiC);

        var writer = new S.Sec { Z = 2, NegiC = 9 };
        var writeArc = new S.Arc();
        S.Secplane(writeArc, "sec", writer, new S.Sec());
        Assert.Equal(9.0, writer.NegiC);
        Assert.Contains("arr:normal", writeArc.Text());
    }
}
