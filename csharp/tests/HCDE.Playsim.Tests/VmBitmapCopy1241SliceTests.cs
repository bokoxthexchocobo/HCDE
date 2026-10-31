using HCDE.Playsim;

namespace HCDE.Playsim.Tests;

public class VmBitmapCopy1241SliceTests
{
    const string HeadGolden =
"""
clip w=2 h=1 ox=0 oy=0 sx=-3 sy=3 at=3
copy ct=0 rot=4 ok 3c3228ff1e140aff0000000000000000
clip w=1 h=2 ox=0 oy=0 sx=-3 sy=3 at=0
copy ct=0 rot=1 ok 1e140aff0000000000000000000000003c3228ff000000000000000000000000
clip w=2 h=1 ox=0 oy=0 sx=3 sy=3 at=0
copy ct=0 rot=8 ok 1e140aff3c3228ff0000000000000000
clip no
copy ct=0 rot=0 no 11111111111111111111111111111111
clip w=1 h=1 ox=0 oy=0 sx=3 sy=3 at=3
copy ct=0 rot=0 ok 3c3228ff000000000000000000000000
intersect x=2 y=3 w=4 h=4 ok=1
intersect x=5 y=0 w=-3 h=2 ok=0
zero rows=2 x=2
row 11110000000011111111111111111111
row 11110000000011111111111111111111
row 11111111111111111111111111111111

"""
;

    const string TailGolden =
"""
---- edge
clip w=1 h=1 ox=0 oy=0 sx=4 sy=4 at=0
copy ct=4 rot=0 ok 000080ff
clip w=1 h=1 ox=0 oy=0 sx=3 sy=3 at=0
copy ct=5 rot=0 ok eb9bffff
clip w=1 h=1 ox=0 oy=0 sx=4 sy=4 at=0
copy ct=6 rot=0 ok 808080ff
clip w=1 h=1 ox=0 oy=0 sx=2 sy=2 at=0
copy ct=10 rot=0 ok 0000f8ff
clip w=1 h=1 ox=0 oy=0 sx=2 sy=2 at=0
copy ct=9 rot=0 ok 404040ff
clip w=1 h=1 ox=0 oy=0 sx=2 sy=2 at=0
copy ct=3 rot=0 ok 09090980
clip w=1 h=1 ox=0 oy=0 sx=3 sy=3 at=0
copy ct=7 rot=0 ok 010203ff
clip w=1 h=1 ox=0 oy=0 sx=4 sy=4 at=0
copy ct=8 rot=0 ok 01020304
clip w=1 h=1 ox=0 oy=0 sx=4 sy=4 at=0
copy ct=2 rot=0 ok 01020304
clip w=1 h=1 ox=0 oy=0 sx=3 sy=3 at=0
copy ct=1 rot=0 ok aaaaaaaa
clip w=1 h=1 ox=0 oy=0 sx=3 sy=3 at=0
copy ct=1 rot=0 ok 040201ff
clip w=1 h=1 ox=0 oy=0 sx=3 sy=3 at=0
copy ct=0 rot=0 ok ac9494ff
clip w=1 h=1 ox=0 oy=0 sx=3 sy=3 at=0
copy ct=0 rot=0 ok 0202f9ff
clip w=1 h=1 ox=0 oy=0 sx=3 sy=3 at=0
copy ct=0 rot=0 ok 010203ff
clip w=1 h=1 ox=0 oy=0 sx=1 sy=1 at=0
pal rebuild
pal op=0 640000ff
clip w=1 h=1 ox=0 oy=0 sx=1 sy=1 at=0
pal op=1 050b1080
clip w=1 h=1 ox=0 oy=0 sx=1 sy=1 at=0
pal op=6 050a1080
clip w=1 h=1 ox=0 oy=0 sx=1 sy=1 at=0
pal op=9 09080700
clip w=1 h=1 ox=0 oy=0 sx=1 sy=1 at=0
pal op=0 0a141e28
clip w=1 h=1 ox=0 oy=0 sx=3 sy=3 at=0
copy ct=0 rot=0 ok ffffffff
clip w=1 h=1 ox=0 oy=0 sx=1 sy=1 at=0
pal op=7 01020340
clip w=1 h=1 ox=0 oy=0 sx=1 sy=1 at=0
pal replace
pal op=0 0000ffff
clip w=1 h=1 ox=0 oy=0 sx=3 sy=3 at=0
blend skip
copy ct=0 rot=0 ok 22222222
clip w=1 h=1 ox=0 oy=0 sx=3 sy=3 at=0
special oob 7
copy ct=0 rot=0 ok 22222222
clip w=1 h=1 ox=0 oy=0 sx=3 sy=3 at=0
dispatch oob ct=12 op=0

"""
;

    [Fact]
    public void Clip_RotatesFlipsAndClearsFromTheFirstRow()
    {
        string dump = Build();
        int cut = dump.IndexOf("---- edge\n", StringComparison.Ordinal);
        string head = dump[..cut];
        Assert.Contains("clip w=2 h=1 ox=0 oy=0 sx=-3 sy=3 at=3\n", head, StringComparison.Ordinal);
        Assert.Contains("copy ct=0 rot=4 ok 3c3228ff1e140aff0000000000000000\n", head, StringComparison.Ordinal);
        Assert.Contains("clip w=1 h=2 ox=0 oy=0 sx=-3 sy=3 at=0\n", head, StringComparison.Ordinal);
        Assert.Contains("copy ct=0 rot=1 ok 1e140aff0000000000000000000000003c3228ff000000000000000000000000\n", head, StringComparison.Ordinal);
        Assert.Contains("copy ct=0 rot=8 ok 1e140aff3c3228ff0000000000000000\n", head, StringComparison.Ordinal);
        Assert.Contains("clip no\n", head, StringComparison.Ordinal);
        Assert.Contains("copy ct=0 rot=0 no 11111111111111111111111111111111\n", head, StringComparison.Ordinal);
        Assert.Contains("clip w=1 h=1 ox=0 oy=0 sx=3 sy=3 at=3\n", head, StringComparison.Ordinal);
        Assert.Contains("intersect x=2 y=3 w=4 h=4 ok=1\n", head, StringComparison.Ordinal);
        Assert.Contains("intersect x=5 y=0 w=-3 h=2 ok=0\n", head, StringComparison.Ordinal);
        Assert.Contains("zero rows=2 x=2\n", head, StringComparison.Ordinal);
        Assert.Contains("row 11110000000011111111111111111111\n", head, StringComparison.Ordinal);
        Assert.Contains("row 11111111111111111111111111111111\n", head, StringComparison.Ordinal);
        Assert.Equal(HeadGolden, head);
    }

    [Fact]
    public void Dispatch_ConvertsFormatsAndAppliesPaletteOps()
    {
        string dump = Build();
        string tail = dump[dump.IndexOf("---- edge\n", StringComparison.Ordinal)..];
        Assert.Contains("copy ct=4 rot=0 ok 000080ff\n", tail, StringComparison.Ordinal);
        Assert.Contains("copy ct=5 rot=0 ok eb9bffff\n", tail, StringComparison.Ordinal);
        Assert.Contains("copy ct=10 rot=0 ok 0000f8ff\n", tail, StringComparison.Ordinal);
        Assert.Contains("copy ct=0 rot=0 ok ac9494ff\n", tail, StringComparison.Ordinal);
        Assert.Contains("copy ct=0 rot=0 ok 0202f9ff\n", tail, StringComparison.Ordinal);
        Assert.Contains("pal rebuild\n", tail, StringComparison.Ordinal);
        Assert.Contains("pal op=0 640000ff\n", tail, StringComparison.Ordinal);
        Assert.Contains("pal op=1 050b1080\n", tail, StringComparison.Ordinal);
        Assert.Contains("pal op=6 050a1080\n", tail, StringComparison.Ordinal);
        Assert.Contains("pal op=9 09080700\n", tail, StringComparison.Ordinal);
        Assert.Contains("pal op=0 0a141e28\n", tail, StringComparison.Ordinal);
        Assert.Contains("copy ct=0 rot=0 ok ffffffff\n", tail, StringComparison.Ordinal);
        Assert.Contains("pal op=7 01020340\n", tail, StringComparison.Ordinal);
        Assert.Contains("copy ct=0 rot=0 ok 010203ff\n", tail, StringComparison.Ordinal);
        Assert.Contains("blend skip\n", tail, StringComparison.Ordinal);
        Assert.Contains("special oob 7\n", tail, StringComparison.Ordinal);
        Assert.Contains("dispatch oob ct=12 op=0\n", tail, StringComparison.Ordinal);
        Assert.Contains("pal replace\n", tail, StringComparison.Ordinal);
        Assert.Equal(TailGolden, tail);
    }

    static string Build()
    {
        var vm = new VmBitmapCopy1241();
        byte[] rgb = { 10, 20, 30, 40, 50, 60 };
        vm.CopyRgb(vm.New(4, 1, 0), 0, 0, rgb, 0, 2, 1, 3, 3, 4, VmBitmapCopy1241.Rgb, null, 0, 0, 0);
        vm.CopyRgb(vm.New(4, 2, 0), 0, 0, rgb, 0, 2, 1, 3, 3, 1, VmBitmapCopy1241.Rgb, null, 0, 0, 0);
        vm.CopyRgb(vm.New(4, 1, 0), 0, 0, rgb, 0, 2, 1, 3, 3, 8, VmBitmapCopy1241.Rgb, null, 0, 0, 0);
        vm.CopyRgb(vm.New(4, 1, 0x11), 10, 0, rgb, 0, 2, 1, 3, 3, 0, VmBitmapCopy1241.Rgb, null, 0, 0, 0);
        vm.CopyRgb(vm.New(4, 1, 0), -1, 0, rgb, 0, 2, 1, 3, 3, 0, VmBitmapCopy1241.Rgb, null, 0, 0, 0);

        var hit = new VmBitmapCopy1241.Rect { X = 0, Y = 0, W = 10, H = 10 };
        vm.Intersect(hit, 2, 3, 4, 4);
        var miss = new VmBitmapCopy1241.Rect { X = 0, Y = 0, W = 2, H = 2 };
        vm.Intersect(miss, 5, 0, 2, 2);

        VmBitmapCopy1241.Surface cleared = vm.New(4, 3, 0x11);
        cleared.Clip.X = 2;
        cleared.Clip.Y = 1;
        cleared.Clip.W = 1;
        cleared.Clip.H = 3;
        vm.Zero(cleared);

        vm.Log.Add("---- edge");

        vm.CopyRgb(vm.New(1, 1, 0), 0, 0, new byte[] { 128, 0, 0, 255 }, 0, 1, 1, 4, 4, 0, VmBitmapCopy1241.Cmyk, null, 0, 0, 0);
        vm.CopyRgb(vm.New(1, 1, 0), 0, 0, new byte[] { 235, 128, 240 }, 0, 1, 1, 3, 3, 0, VmBitmapCopy1241.Ycbcr, null, 0, 0, 0);
        vm.CopyRgb(vm.New(1, 1, 0), 0, 0, new byte[] { 128, 128, 128, 255 }, 0, 1, 1, 4, 4, 0, VmBitmapCopy1241.Ycck, null, 0, 0, 0);
        vm.CopyRgb(vm.New(1, 1, 0), 0, 0, new byte[] { 0x1f, 0x00 }, 0, 1, 1, 2, 2, 0, VmBitmapCopy1241.Rgb555, null, 0, 0, 0);
        vm.CopyRgb(vm.New(1, 1, 0), 0, 0, new byte[] { 0, 0x40 }, 0, 1, 1, 2, 2, 0, VmBitmapCopy1241.I16, null, 0, 0, 0);
        vm.CopyRgb(vm.New(1, 1, 0), 0, 0, new byte[] { 9, 128 }, 0, 1, 1, 2, 2, 0, VmBitmapCopy1241.Ia, null, 0, 0, 0);
        vm.CopyRgb(vm.New(1, 1, 0), 0, 0, new byte[] { 1, 2, 3 }, 0, 1, 1, 3, 3, 0, VmBitmapCopy1241.Bgr, null, 0, 0, 0);
        vm.CopyRgb(vm.New(1, 1, 0), 0, 0, new byte[] { 1, 2, 3, 4 }, 0, 1, 1, 4, 4, 0, VmBitmapCopy1241.Bgra, null, 0, 0, 0);
        vm.CopyRgb(vm.New(1, 1, 0), 0, 0, new byte[] { 3, 2, 1, 4 }, 0, 1, 1, 4, 4, 0, VmBitmapCopy1241.Rgba, null, 0, 0, 0);
        vm.CopyRgb(vm.New(1, 1, 0xaa), 0, 0, new byte[] { 1, 2, 3 }, 0, 1, 1, 3, 3, 0, VmBitmapCopy1241.RgbT, null, 1, 2, 3);
        vm.CopyRgb(vm.New(1, 1, 0xaa), 0, 0, new byte[] { 1, 2, 4 }, 0, 1, 1, 3, 3, 0, VmBitmapCopy1241.RgbT, null, 1, 2, 3);

        var ice = new VmBitmapCopy1241.Info { Blend = 1 };
        vm.CopyRgb(vm.New(1, 1, 0), 0, 0, new byte[] { 255, 255, 255 }, 0, 1, 1, 3, 3, 0, VmBitmapCopy1241.Rgb, ice, 0, 0, 0);
        var desat = new VmBitmapCopy1241.Info { Blend = 2 };
        vm.CopyRgb(vm.New(1, 1, 0), 0, 0, new byte[] { 255, 0, 0 }, 0, 1, 1, 3, 3, 0, VmBitmapCopy1241.Rgb, desat, 0, 0, 0);
        var special = new uint[256];
        special[76] = Pack(1, 2, 3, 255);
        var spec = new VmBitmapCopy1241.Info { Blend = 33, Special = special };
        vm.CopyRgb(vm.New(1, 1, 0), 0, 0, new byte[] { 255, 0, 0 }, 0, 1, 1, 3, 3, 0, VmBitmapCopy1241.Rgb, spec, 0, 0, 0);

        var half = new VmBitmapCopy1241.Info { Blend = -1, Color = new[] { 32768, 32768, 32768, 0 } };
        var pal = new uint[256];
        pal[1] = Pack(200, 0, 0, 255);
        vm.CopyPal(vm.New(1, 1, 0), 0, 0, new byte[] { 1 }, 0, 1, 1, 1, 1, 0, pal, half);

        var blend = new VmBitmapCopy1241.Info { Op = VmBitmapCopy1241.Blend, Alpha = 32768, InvAlpha = 32768 };
        var tint = new uint[256];
        tint[1] = Pack(1, 2, 3, 128);
        VmBitmapCopy1241.Surface blended = vm.New(1, 1, 0);
        blended.Data[0] = 10;
        blended.Data[1] = 20;
        blended.Data[2] = 30;
        blended.Data[3] = 40;
        vm.CopyPal(blended, 0, 0, new byte[] { 1 }, 0, 1, 1, 1, 1, 0, tint, blend);

        var copyA = new VmBitmapCopy1241.Info { Op = VmBitmapCopy1241.CopyAlpha };
        VmBitmapCopy1241.Surface mixed = vm.New(1, 1, 0);
        mixed.Data[0] = 10;
        mixed.Data[1] = 20;
        mixed.Data[2] = 30;
        mixed.Data[3] = 40;
        vm.CopyPal(mixed, 0, 0, new byte[] { 1 }, 0, 1, 1, 1, 1, 0, tint, copyA);

        var clear = new uint[256];
        clear[1] = Pack(9, 8, 7, 0);
        var over = new VmBitmapCopy1241.Info { Op = VmBitmapCopy1241.Overwrite };
        VmBitmapCopy1241.Surface hole = vm.New(1, 1, 0);
        hole.Data[0] = 10;
        hole.Data[1] = 20;
        hole.Data[2] = 30;
        hole.Data[3] = 40;
        vm.CopyPal(hole, 0, 0, new byte[] { 1 }, 0, 1, 1, 1, 1, 0, clear, over);
        VmBitmapCopy1241.Surface kept = vm.New(1, 1, 0);
        kept.Data[0] = 10;
        kept.Data[1] = 20;
        kept.Data[2] = 30;
        kept.Data[3] = 40;
        vm.CopyPal(kept, 0, 0, new byte[] { 1 }, 0, 1, 1, 1, 1, 0, clear, null);

        var add = new VmBitmapCopy1241.Info { Op = VmBitmapCopy1241.Add, Alpha = 65536 };
        VmBitmapCopy1241.Surface summed = vm.New(1, 1, 200);
        vm.CopyRgb(summed, 0, 0, new byte[] { 100, 100, 100 }, 0, 1, 1, 3, 3, 0, VmBitmapCopy1241.Rgb, add, 0, 0, 0);

        var newer = new VmBitmapCopy1241.Info { Op = VmBitmapCopy1241.CopyNewAlpha, Alpha = 32768 };
        var ink = new uint[256];
        ink[1] = Pack(1, 2, 3, 128);
        vm.CopyPal(vm.New(1, 1, 0), 0, 0, new byte[] { 1 }, 0, 1, 1, 1, 1, 0, ink, newer);

        var swapped = new uint[256];
        swapped[1] = Pack(0, 0, 255, 255);
        var replace = new VmBitmapCopy1241.Info { Palette = swapped };
        var ignored = new uint[256];
        ignored[1] = Pack(255, 0, 0, 255);
        vm.CopyPal(vm.New(1, 1, 0), 0, 0, new byte[] { 1 }, 0, 1, 1, 1, 1, 0, ignored, replace);

        var dead = new VmBitmapCopy1241.Info { Blend = -3 };
        vm.CopyRgb(vm.New(1, 1, 0x22), 0, 0, new byte[] { 1, 2, 3 }, 0, 1, 1, 3, 3, 0, VmBitmapCopy1241.Rgb, dead, 0, 0, 0);
        var missing = new VmBitmapCopy1241.Info { Blend = 40 };
        vm.CopyRgb(vm.New(1, 1, 0x22), 0, 0, new byte[] { 1, 2, 3 }, 0, 1, 1, 3, 3, 0, VmBitmapCopy1241.Rgb, missing, 0, 0, 0);
        vm.CopyRgb(vm.New(1, 1, 0x22), 0, 0, new byte[] { 1, 2, 3 }, 0, 1, 1, 3, 3, 0, 12, null, 0, 0, 0);

        string dump = string.Join("\n", vm.Log) + "\n";
        return dump;
    }

    static uint Pack(byte b, byte g, byte r, byte a) =>
        (uint)(b | (g << 8) | (r << 16) | (a << 24));
}
