using HCDE.Playsim;

namespace HCDE.Playsim.Tests;

public class VmMultiPatch1238SliceTests
{
    [Fact]
    public void PaletteVote_CompositesPostsAndClearsStyleMaps()
    {
        string dump = Build();
        int cut = dump.IndexOf("---- edge\n", StringComparison.Ordinal);
        string head = dump[..cut];
        Assert.Contains("make id=1 w=2 h=2 cx=1 tx=0 pal=0 n=1\n", head, StringComparison.Ordinal);
        Assert.Contains("remap0 id=1 1\n", head, StringComparison.Ordinal);
        Assert.Contains("remap0 id=3 0\n", head, StringComparison.Ordinal);
        Assert.Contains("remap0 id=6 1\n", head, StringComparison.Ordinal);
        Assert.Contains("style clear\n", head, StringComparison.Ordinal);
        Assert.Contains("part 0 ice\n", head, StringComparison.Ordinal);
        Assert.Contains("part 0 null\n", head, StringComparison.Ordinal);
        Assert.Contains("clip miss x=5 y=0\n", head, StringComparison.Ordinal);
        Assert.Contains("build=pal ", head, StringComparison.Ordinal);
        Assert.Contains("pix 000001020003\n", head, StringComparison.Ordinal);
        Assert.Contains("pix 02030100\n", head, StringComparison.Ordinal);
        Assert.Contains("pix 39\n", head, StringComparison.Ordinal);
        Assert.Contains("pix d0\n", head, StringComparison.Ordinal);
        Assert.DoesNotContain("build=rgb", head, StringComparison.Ordinal);
        Assert.Equal(HeadGolden, head);
    }

    [Fact]
    public void TrueColor_BlendsDownconvertsAndKeepsUnknownReturn()
    {
        string dump = Build();
        string tail = dump[dump.IndexOf("---- edge\n", StringComparison.Ordinal)..];
        Assert.Contains("retv -1\n", tail, StringComparison.Ordinal);
        Assert.Contains("special oob 40\n", tail, StringComparison.Ordinal);
        Assert.Contains("build=rgb ", tail, StringComparison.Ordinal);
        Assert.Contains("pix bd3b00b9\n", tail, StringComparison.Ordinal);
        Assert.Contains("bmp 0f1e2dff\n", tail, StringComparison.Ordinal);
        Assert.Contains("bmp 241014ff\n", tail, StringComparison.Ordinal);
        Assert.Contains("bmp 09090900\n", tail, StringComparison.Ordinal);
        Assert.Contains("bmp 000000ff\n", tail, StringComparison.Ordinal);
        Assert.Contains("copy id=58 asked=2 used=2 frame=9\n", tail, StringComparison.Ordinal);
        Assert.Contains("copy id=58 asked=1 used=1 frame=2\n", tail, StringComparison.Ordinal);
        Assert.Contains("child=1\n", tail, StringComparison.Ordinal);
        Assert.Contains("cache id=134 conv=0 ret=0 pal=1\n", tail, StringComparison.Ordinal);
        Assert.DoesNotContain("retv 0\n", tail, StringComparison.Ordinal);
        Assert.DoesNotContain("retv 4\n", tail, StringComparison.Ordinal);
        Assert.DoesNotContain("retv 5\n", tail, StringComparison.Ordinal);
        Assert.Equal(TailGolden, tail);
    }

    static string Build()
    {
        var vm = new VmMultiPatch1238();
        var log = vm.Log;
        var quad = S(100, new byte[] { 1, 2, 0, 3 }, 2, 2);
        var foreign = S(101, new byte[] { 4 }, 1, 1, game: false, remap0: false);
        var one = S(102, new byte[] { 5 }, 1, 1);
        var zero = S(103, new byte[] { 0 }, 1, 1);
        var nine = S(104, new byte[] { 9 }, 1, 1);
        var odd = S(105, new byte[] { 1, 2, 0, 3 }, 2, 2, game: false);
        var column = S(106, new byte[] { 1, 2 }, 1, 2);
        byte[] map = XorMap(0x11);

        vm.Remap0(vm.Make(2, 2, true, false, new[] { P(quad) }));
        vm.Remap0(vm.Make(1, 1, false, false, new[] { P(quad), P(foreign) }));
        vm.Remap0(vm.Make(1, 1, false, false, new[] { P(one) }));
        vm.Remap0(vm.Make(1, 1, false, true, new[] { P(one) }));
        vm.Remap0(vm.Make(0, 0, false, false, Array.Empty<VmMultiPatch1238.Part>()));
        vm.Remap0(vm.Make(0, 0, true, false, Array.Empty<VmMultiPatch1238.Part>()));

        vm.Pixels(vm.Make(3, 2, false, false, new[] { P(quad, x: 1) }), VmMultiPatch1238.Normal, 3);
        vm.Pixels(vm.Make(2, 2, false, false, new[] { P(quad, x: -1) }), VmMultiPatch1238.Normal, 0);
        vm.Pixels(vm.Make(2, 2, false, false, new[] { P(quad, rot: 1) }), VmMultiPatch1238.Normal, 0);
        vm.Pixels(vm.Make(1, 1, false, false, new[] { P(column, y: -1) }), VmMultiPatch1238.Normal, 0);

        var translated = vm.Make(2, 2, false, false, new[] { P(quad, remap: map) });
        vm.Pixels(translated, VmMultiPatch1238.Normal, 0);
        vm.Pixels(translated, VmMultiPatch1238.Luminance, 7);
        vm.Pixels(translated, VmMultiPatch1238.NoRemap0, 1);
        vm.Pixels(vm.Make(2, 2, false, true, new[] { P(quad, remap: map) }), VmMultiPatch1238.NoRemap0, 2);
        vm.Pixels(vm.Make(2, 2, false, false, new[] { P(odd, remap: map) }), VmMultiPatch1238.NoRemap0, 4);

        vm.Pixels(vm.Make(1, 1, false, false, new[] { P(one, blend: 1) }), VmMultiPatch1238.Normal, 0);
        vm.Pixels(vm.Make(1, 1, false, false, new[] { P(one, blend: 1) }), VmMultiPatch1238.Luminance, 0);
        vm.Pixels(vm.Make(1, 1, false, false, new[] { P(one, blend: 2) }), VmMultiPatch1238.Normal, 0);
        vm.Pixels(vm.Make(1, 1, false, false, new[] { P(one, blend: 32) }), VmMultiPatch1238.Normal, 0);
        vm.Pixels(vm.Make(1, 1, false, false, new[] { P(one, blend: 33) }), VmMultiPatch1238.Normal, 0);
        vm.Pixels(vm.Make(1, 1, false, false, new[] { P(one, blend: 36) }), VmMultiPatch1238.Normal, 0);
        vm.Pixels(vm.Make(1, 1, false, false, new[] { P(one, blend: VmMultiPatch1238.Pack(255, 255, 0, 0)) }), VmMultiPatch1238.Normal, 0);
        vm.Pixels(vm.Make(1, 1, false, false, new[] { P(one, blend: VmMultiPatch1238.Pack(128, 10, 20, 30)) }), VmMultiPatch1238.Normal, 0);
        vm.Pixels(vm.Make(1, 1, false, false, new[] { P(one, blend: 40) }), VmMultiPatch1238.Normal, 0);
        vm.Pixels(vm.Make(1, 1, false, false, new[] { P(one, remap: map, blend: 1) }), VmMultiPatch1238.Normal, 0);

        byte[] kill = new byte[256];
        vm.Pixels(vm.Make(1, 1, false, false, new[] { P(one, remap: kill), P(nine) }), VmMultiPatch1238.Normal, 0);
        vm.Pixels(vm.Make(1, 1, false, false, new[] { P(nine), P(zero) }), VmMultiPatch1238.Normal, 0);
        vm.Pixels(vm.Make(1, 1, false, false, new[] { P(one, op: VmMultiPatch1238.OpAdd) }), VmMultiPatch1238.Luminance, 0);
        vm.Pixels(vm.Make(1, 1, false, false, new[] { P(one, x: 5) }), VmMultiPatch1238.Normal, 0);
        vm.Pixels(vm.Make(2, 2, false, false, new[] { P(odd) }), VmMultiPatch1238.Normal, 0);

        log.Add("---- edge");

        var dot = S(110, new byte[] { 1 }, 1, 1, rgba: new byte[] { 10, 20, 30, 255 });
        var black = S(111, new byte[] { 1 }, 1, 1, rgba: new byte[] { 0, 0, 0, 255 });
        var clear = S(112, new byte[] { 1 }, 1, 1, rgba: new byte[] { 9, 9, 9, 0 });
        var half = S(113, new byte[] { 1 }, 1, 1, rgba: new byte[] { 0, 0, 0, 128 });
        var red = S(114, new byte[] { 1 }, 1, 1, rgba: new byte[] { 255, 0, 0, 255 });
        var wide = S(115, new byte[] { 1, 2 }, 2, 1, rgba: new byte[] { 1, 2, 3, 255, 4, 5, 6, 255 });
        var raw = S(132, new byte[] { 7 }, 1, 1, game: false, rgba: new byte[] { 8, 0, 0, 255 });

        var complex = vm.Make(1, 1, true, false, new[] { P(dot) });
        vm.Remap0(complex);
        vm.Pixels(complex, VmMultiPatch1238.Normal, 8);
        vm.Pixels(complex, VmMultiPatch1238.Luminance, 8);
        var grid = S(140, new byte[] { 1, 2, 3, 4 }, 2, 2, rgba: new byte[]
        {
            0, 0, 255, 255, 0, 0, 0, 0, 0, 255, 0, 255, 255, 0, 0, 128
        });
        vm.Pixels(vm.Make(2, 2, true, false, new[] { P(grid) }), VmMultiPatch1238.Normal, 0);
        vm.Pixels(vm.Make(1, 1, true, false, new[] { P(dot) }), VmMultiPatch1238.NoRemap0, 5);
        vm.Pixels(vm.Make(1, 1, false, false, new[] { P(raw) }), VmMultiPatch1238.Luminance, 1);
        vm.Pixels(vm.Make(1, 1, false, false, new[] { P(one, op: VmMultiPatch1238.OpAdd) }), VmMultiPatch1238.Normal, 0);

        Copy(vm, vm.Make(1, 1, false, false, new[] { P(dot), P(dot, op: VmMultiPatch1238.OpAdd, alpha: 32768) }));
        Copy(vm, vm.Make(1, 1, false, false, new[] { P(dot), P(black, op: VmMultiPatch1238.OpBlend, alpha: 32768) }));
        Copy(vm, vm.Make(1, 1, false, false, new[] { P(dot), P(dot, op: VmMultiPatch1238.OpSubtract, alpha: 32768) }));
        Copy(vm, vm.Make(1, 1, false, false, new[] { P(dot, op: VmMultiPatch1238.OpReverseSubtract, alpha: 32768) }));
        Copy(vm, vm.Make(1, 1, false, false, new[] { P(dot), P(dot, op: VmMultiPatch1238.OpReverseSubtract, alpha: 32768) }));
        Copy(vm, vm.Make(1, 1, false, false, new[] { P(dot), P(dot, op: VmMultiPatch1238.OpModulate) }));
        Copy(vm, vm.Make(1, 1, false, false, new[] { P(dot), P(half, op: VmMultiPatch1238.OpCopyAlpha) }));
        Copy(vm, vm.Make(1, 1, false, false, new[] { P(dot, op: VmMultiPatch1238.OpCopyNewAlpha, alpha: 32768) }));
        Copy(vm, vm.Make(1, 1, false, false, new[] { P(dot), P(half, op: VmMultiPatch1238.OpOverlay) }));
        Copy(vm, vm.Make(1, 1, false, false, new[] { P(dot), P(clear, op: VmMultiPatch1238.OpOverwrite) }));
        Copy(vm, vm.Make(1, 1, false, false, new[] { P(dot), P(clear) }));

        Copy(vm, vm.Make(1, 1, false, false, new[] { P(red, blend: 1) }));
        Copy(vm, vm.Make(1, 1, false, false, new[] { P(red, blend: 32) }));
        Copy(vm, vm.Make(1, 1, false, false, new[] { P(red, blend: 33) }));
        Copy(vm, vm.Make(1, 1, false, false, new[] { P(red, blend: 36) }));
        Copy(vm, vm.Make(1, 1, false, false, new[] { P(red, blend: 40) }));
        Copy(vm, vm.Make(1, 1, false, false, new[] { P(dot, blend: VmMultiPatch1238.Pack(255, 255, 128, 0)) }));
        Copy(vm, vm.Make(1, 1, false, false, new[] { P(dot, blend: VmMultiPatch1238.Pack(128, 10, 20, 30)) }));
        Copy(vm, vm.Make(2, 1, false, false, new[] { P(wide, rot: 4) }));
        Copy(vm, vm.Make(1, 1, false, false, new[] { P(dot, x: 3) }));

        var unknown = S(117, new byte[] { 1 }, 1, 1, rgba: new byte[] { 1, 0, 0, 255 }, trans: -1);
        var mid = S(118, new byte[] { 1 }, 1, 1, rgba: new byte[] { 2, 0, 0, 255 }, trans: 4);
        var low = S(119, new byte[] { 1 }, 1, 1, rgba: new byte[] { 3, 0, 0, 255 }, trans: 2);
        Copy(vm, vm.Make(1, 1, false, false, new[] { P(unknown), P(mid), P(low) }), frame: 4);
        var hi = S(120, new byte[] { 1 }, 1, 1, rgba: new byte[] { 4, 0, 0, 255 }, trans: 1);
        var higher = S(121, new byte[] { 1 }, 1, 1, rgba: new byte[] { 5, 0, 0, 255 }, trans: 5);
        Copy(vm, vm.Make(1, 1, false, false, new[] { P(hi), P(higher) }));
        Copy(vm, vm.Make(1, 1, false, false, Array.Empty<VmMultiPatch1238.Part>()));

        var textual = vm.Make(1, 1, false, true, new[] { P(dot) });
        Copy(vm, textual, VmMultiPatch1238.NoRemap0, 9);
        var plain = vm.Make(1, 1, false, false, new[] { P(dot) });
        Copy(vm, plain, VmMultiPatch1238.NoRemap0, 9);
        Copy(vm, plain, VmMultiPatch1238.Luminance, 2);

        uint[] pal = new uint[256];
        pal[5] = VmMultiPatch1238.Pack(255, 1, 2, 3);
        var indexed = S(134, new byte[] { 5 }, 1, 1);
        Copy(vm, vm.Make(1, 1, false, false, new[] { P(indexed, palette: pal) }));
        uint[] dead = new uint[256];
        dead[0] = VmMultiPatch1238.Pack(0, 9, 9, 9);
        var blank = S(135, new byte[] { 0 }, 1, 1);
        Copy(vm, vm.Make(1, 1, false, false, new[] { P(blank, palette: dead) }));

        var leaf = S(130, new byte[] { 1 }, 1, 1);
        var simple = vm.Make(1, 1, false, false, new[] { P(leaf) });
        vm.Precache(simple, false);
        vm.Precache(vm.Make(1, 1, false, false, new[] { P(leaf, op: VmMultiPatch1238.OpAdd) }), false);
        vm.Precache(vm.Make(1, 1, true, false, new[] { P(leaf) }), false);
        vm.Precache(simple, true);
        var pair = S(131, new byte[] { 1 }, 1, 1);
        vm.Precache(vm.Make(1, 1, false, false, new[] { P(pair), P(pair) }), false);

        return string.Join('\n', log) + "\n";
    }

    static void Copy(VmMultiPatch1238 vm, VmMultiPatch1238.Texture tex, int conversion = VmMultiPatch1238.Normal, int frame = 0)
    {
        vm.Copy(tex, conversion, frame, out _);
    }

    static VmMultiPatch1238.Source S(int id, byte[] pal, int w, int h, bool game = true, bool remap0 = true, byte[]? rgba = null, int trans = 0)
    {
        return new VmMultiPatch1238.Source
        {
            Id = id,
            Width = w,
            Height = h,
            GamePalette = game,
            Remap0 = remap0,
            Pal = pal,
            Rgba = rgba,
            Trans = trans
        };
    }

    static VmMultiPatch1238.Part P(
        VmMultiPatch1238.Source image,
        int x = 0,
        int y = 0,
        int rot = 0,
        int op = 0,
        int alpha = VmMultiPatch1238.BlendUnit,
        uint blend = 0,
        byte[]? remap = null,
        uint[]? palette = null)
    {
        return new VmMultiPatch1238.Part
        {
            Image = image,
            OriginX = x,
            OriginY = y,
            Rotate = rot,
            Op = op,
            Alpha = alpha,
            Blend = blend,
            Remap = remap,
            Palette = palette
        };
    }

    static byte[] XorMap(int mask)
    {
        var map = new byte[256];
        for (int i = 0; i < 256; i++)
            map[i] = (byte)(i ^ mask);
        return map;
    }

    const string HeadGolden =
"""
make id=1 w=2 h=2 cx=1 tx=0 pal=0 n=1
remap0 id=1 1
make id=2 w=1 h=1 cx=0 tx=0 pal=0 n=2
remap0 id=2 0
make id=3 w=1 h=1 cx=0 tx=0 pal=1 n=1
remap0 id=3 0
make id=4 w=1 h=1 cx=0 tx=1 pal=1 n=1
remap0 id=4 0
make id=5 w=0 h=0 cx=0 tx=0 pal=1 n=0
remap0 id=5 0
make id=6 w=0 h=0 cx=1 tx=0 pal=0 n=0
remap0 id=6 1
make id=7 w=3 h=2 cx=0 tx=0 pal=1 n=1
pixels id=7 asked=0 used=0 frame=3 build=pal pal=1
part 0 off
block x=1 y=0 w=2 h=2 rot=0 map=0
pix 000001020003
make id=8 w=2 h=2 cx=0 tx=0 pal=1 n=1
pixels id=8 asked=0 used=0 frame=0 build=pal pal=1
part 0 off
block x=0 y=0 w=1 h=2 rot=0 map=0
pix 00030000
make id=9 w=2 h=2 cx=0 tx=0 pal=1 n=1
pixels id=9 asked=0 used=0 frame=0 build=pal pal=1
part 0 off
block x=0 y=0 w=2 h=2 rot=1 map=0
pix 02030100
make id=10 w=1 h=1 cx=0 tx=0 pal=1 n=1
pixels id=10 asked=0 used=0 frame=0 build=pal pal=1
part 0 off
block x=0 y=0 w=1 h=1 rot=0 map=0
pix 02
make id=11 w=2 h=2 cx=0 tx=0 pal=1 n=1
pixels id=11 asked=0 used=0 frame=0 build=pal pal=1
part 0 off
block x=0 y=0 w=2 h=2 rot=0 map=1
pix 10130012
pixels id=11 asked=1 used=1 frame=7 build=pal pal=1
part 0 off
style clear
block x=0 y=0 w=2 h=2 rot=0 map=0
pix 01020003
pixels id=11 asked=2 used=2 frame=1 build=pal pal=1
part 0 off
style clear
block x=0 y=0 w=2 h=2 rot=0 map=0
pix 01020003
make id=12 w=2 h=2 cx=0 tx=1 pal=1 n=1
pixels id=12 asked=2 used=0 frame=2 build=pal pal=1
part 0 off
block x=0 y=0 w=2 h=2 rot=0 map=1
pix 10130012
make id=13 w=2 h=2 cx=0 tx=0 pal=0 n=1
pixels id=13 asked=2 used=0 frame=4 build=pal pal=0
part 0 off
block x=0 y=0 w=2 h=2 rot=0 map=1
pix 10130012
make id=14 w=1 h=1 cx=0 tx=0 pal=1 n=1
pixels id=14 asked=0 used=0 frame=0 build=pal pal=1
part 0 ice
block x=0 y=0 w=1 h=1 rot=0 map=1
pix 39
make id=15 w=1 h=1 cx=0 tx=0 pal=1 n=1
pixels id=15 asked=1 used=1 frame=0 build=pal pal=1
part 0 ice
style clear
block x=0 y=0 w=1 h=1 rot=0 map=0
pix 05
make id=16 w=1 h=1 cx=0 tx=0 pal=1 n=1
pixels id=16 asked=0 used=0 frame=0 build=pal pal=1
part 0 desat 0
block x=0 y=0 w=1 h=1 rot=0 map=1
pix 25
make id=17 w=1 h=1 cx=0 tx=0 pal=1 n=1
pixels id=17 asked=0 used=0 frame=0 build=pal pal=1
part 0 desat 30
block x=0 y=0 w=1 h=1 rot=0 map=1
pix 3b
make id=18 w=1 h=1 cx=0 tx=0 pal=1 n=1
pixels id=18 asked=0 used=0 frame=0 build=pal pal=1
part 0 special 0
block x=0 y=0 w=1 h=1 rot=0 map=1
pix 0e
make id=19 w=1 h=1 cx=0 tx=0 pal=1 n=1
pixels id=19 asked=0 used=0 frame=0 build=pal pal=1
part 0 special 3
block x=0 y=0 w=1 h=1 rot=0 map=1
pix 29
make id=20 w=1 h=1 cx=0 tx=0 pal=1 n=1
pixels id=20 asked=0 used=0 frame=0 build=pal pal=1
part 0 mod
block x=0 y=0 w=1 h=1 rot=0 map=1
pix 15
make id=21 w=1 h=1 cx=0 tx=0 pal=1 n=1
pixels id=21 asked=0 used=0 frame=0 build=pal pal=1
part 0 over
block x=0 y=0 w=1 h=1 rot=0 map=1
pix d0
make id=22 w=1 h=1 cx=0 tx=0 pal=1 n=1
pixels id=22 asked=0 used=0 frame=0 build=pal pal=1
part 0 null
block x=0 y=0 w=1 h=1 rot=0 map=0
pix 05
make id=23 w=1 h=1 cx=0 tx=0 pal=1 n=1
pixels id=23 asked=0 used=0 frame=0 build=pal pal=1
part 0 ice
block x=0 y=0 w=1 h=1 rot=0 map=1
pix 39
make id=24 w=1 h=1 cx=0 tx=0 pal=1 n=2
pixels id=24 asked=0 used=0 frame=0 build=pal pal=1
part 0 off
block x=0 y=0 w=1 h=1 rot=0 map=1
part 1 off
block x=0 y=0 w=1 h=1 rot=0 map=0
pix 09
make id=25 w=1 h=1 cx=0 tx=0 pal=1 n=2
pixels id=25 asked=0 used=0 frame=0 build=pal pal=1
part 0 off
block x=0 y=0 w=1 h=1 rot=0 map=0
part 1 off
block x=0 y=0 w=1 h=1 rot=0 map=0
pix 09
make id=26 w=1 h=1 cx=0 tx=0 pal=1 n=1
pixels id=26 asked=1 used=1 frame=0 build=pal pal=1
part 0 off
block x=0 y=0 w=1 h=1 rot=0 map=0
pix 05
make id=27 w=1 h=1 cx=0 tx=0 pal=1 n=1
pixels id=27 asked=0 used=0 frame=0 build=pal pal=1
part 0 off
clip miss x=5 y=0
pix 00
make id=28 w=2 h=2 cx=0 tx=0 pal=0 n=1
pixels id=28 asked=0 used=0 frame=0 build=pal pal=0
part 0 off
block x=0 y=0 w=2 h=2 rot=0 map=0
pix 01020003

""";

    const string TailGolden =
"""
---- edge
make id=29 w=1 h=1 cx=1 tx=0 pal=0 n=1
remap0 id=29 1
pixels id=29 asked=0 used=0 frame=8 build=rgb pal=0
copy id=29 asked=0 used=0 frame=0
cache id=110 conv=0 ret=0 pal=0
info i=0 op=0 a=65536 inv=0 blend=0 bc=0,0,0,0 ret=0
blit x=0 y=0 w=1 h=1 rot=0
retv -1
bmp 0a141eff
pix 3c
pixels id=29 asked=1 used=1 frame=8 build=rgb pal=0
copy id=29 asked=0 used=0 frame=0
cache id=110 conv=0 ret=0 pal=0
info i=0 op=0 a=65536 inv=0 blend=0 bc=0,0,0,0 ret=0
blit x=0 y=0 w=1 h=1 rot=0
retv -1
bmp 0a141eff
pix 15
make id=30 w=2 h=2 cx=1 tx=0 pal=0 n=1
pixels id=30 asked=0 used=0 frame=0 build=rgb pal=0
copy id=30 asked=0 used=0 frame=0
cache id=140 conv=0 ret=0 pal=0
info i=0 op=0 a=65536 inv=0 blend=0 bc=0,0,0,0 ret=0
blit x=0 y=0 w=2 h=2 rot=0
retv -1
bmp 0000ffff0000000000ff00ffff000080
pix bd3b00b9
make id=31 w=1 h=1 cx=1 tx=0 pal=0 n=1
pixels id=31 asked=2 used=0 frame=5 build=rgb pal=0
copy id=31 asked=0 used=0 frame=0
cache id=110 conv=0 ret=0 pal=0
info i=0 op=0 a=65536 inv=0 blend=0 bc=0,0,0,0 ret=0
blit x=0 y=0 w=1 h=1 rot=0
retv -1
bmp 0a141eff
pix 3c
make id=32 w=1 h=1 cx=0 tx=0 pal=0 n=1
pixels id=32 asked=1 used=1 frame=1 build=rgb pal=0
copy id=32 asked=0 used=0 frame=0
cache id=132 conv=0 ret=0 pal=0
info i=0 op=0 a=65536 inv=0 blend=0 bc=0,0,0,0 ret=0
blit x=0 y=0 w=1 h=1 rot=0
retv -1
bmp 080000ff
pix 01
make id=33 w=1 h=1 cx=0 tx=0 pal=1 n=1
pixels id=33 asked=0 used=0 frame=0 build=rgb pal=1
copy id=33 asked=0 used=0 frame=0
cache id=102 conv=0 ret=0 pal=0
info i=0 op=2 a=65536 inv=0 blend=0 bc=0,0,0,0 ret=0
blit x=0 y=0 w=1 h=1 rot=0
retv -1
bmp 050607ff
pix 0f
make id=34 w=1 h=1 cx=0 tx=0 pal=1 n=2
copy id=34 asked=0 used=0 frame=0
cache id=110 conv=0 ret=0 pal=0
info i=0 op=0 a=65536 inv=0 blend=0 bc=0,0,0,0 ret=0
blit x=0 y=0 w=1 h=1 rot=0
cache id=110 conv=0 ret=0 pal=0
info i=1 op=2 a=32768 inv=32768 blend=0 bc=0,0,0,0 ret=0
blit x=0 y=0 w=1 h=1 rot=0
retv -1
bmp 0f1e2dff
make id=35 w=1 h=1 cx=0 tx=0 pal=1 n=2
copy id=35 asked=0 used=0 frame=0
cache id=110 conv=0 ret=0 pal=0
info i=0 op=0 a=65536 inv=0 blend=0 bc=0,0,0,0 ret=0
blit x=0 y=0 w=1 h=1 rot=0
cache id=111 conv=0 ret=0 pal=0
info i=1 op=1 a=32768 inv=32768 blend=0 bc=0,0,0,0 ret=0
blit x=0 y=0 w=1 h=1 rot=0
retv -1
bmp 050a0fff
make id=36 w=1 h=1 cx=0 tx=0 pal=1 n=2
copy id=36 asked=0 used=0 frame=0
cache id=110 conv=0 ret=0 pal=0
info i=0 op=0 a=65536 inv=0 blend=0 bc=0,0,0,0 ret=0
blit x=0 y=0 w=1 h=1 rot=0
cache id=110 conv=0 ret=0 pal=0
info i=1 op=3 a=32768 inv=32768 blend=0 bc=0,0,0,0 ret=0
blit x=0 y=0 w=1 h=1 rot=0
retv -1
bmp 050a0fff
make id=37 w=1 h=1 cx=0 tx=0 pal=1 n=1
copy id=37 asked=0 used=0 frame=0
cache id=110 conv=0 ret=0 pal=0
info i=0 op=4 a=32768 inv=32768 blend=0 bc=0,0,0,0 ret=0
blit x=0 y=0 w=1 h=1 rot=0
retv -1
bmp 050a0fff
make id=38 w=1 h=1 cx=0 tx=0 pal=1 n=2
copy id=38 asked=0 used=0 frame=0
cache id=110 conv=0 ret=0 pal=0
info i=0 op=0 a=65536 inv=0 blend=0 bc=0,0,0,0 ret=0
blit x=0 y=0 w=1 h=1 rot=0
cache id=110 conv=0 ret=0 pal=0
info i=1 op=4 a=32768 inv=32768 blend=0 bc=0,0,0,0 ret=0
blit x=0 y=0 w=1 h=1 rot=0
retv -1
bmp 000000ff
make id=39 w=1 h=1 cx=0 tx=0 pal=1 n=2
copy id=39 asked=0 used=0 frame=0
cache id=110 conv=0 ret=0 pal=0
info i=0 op=0 a=65536 inv=0 blend=0 bc=0,0,0,0 ret=0
blit x=0 y=0 w=1 h=1 rot=0
cache id=110 conv=0 ret=0 pal=0
info i=1 op=5 a=65536 inv=0 blend=0 bc=0,0,0,0 ret=0
blit x=0 y=0 w=1 h=1 rot=0
retv -1
bmp 000103ff
make id=40 w=1 h=1 cx=0 tx=0 pal=1 n=2
copy id=40 asked=0 used=0 frame=0
cache id=110 conv=0 ret=0 pal=0
info i=0 op=0 a=65536 inv=0 blend=0 bc=0,0,0,0 ret=0
blit x=0 y=0 w=1 h=1 rot=0
cache id=113 conv=0 ret=0 pal=0
info i=1 op=6 a=65536 inv=0 blend=0 bc=0,0,0,0 ret=0
blit x=0 y=0 w=1 h=1 rot=0
retv -1
bmp 04090e80
make id=41 w=1 h=1 cx=0 tx=0 pal=1 n=1
copy id=41 asked=0 used=0 frame=0
cache id=110 conv=0 ret=0 pal=0
info i=0 op=7 a=32768 inv=32768 blend=0 bc=0,0,0,0 ret=0
blit x=0 y=0 w=1 h=1 rot=0
retv -1
bmp 0a141e7f
make id=42 w=1 h=1 cx=0 tx=0 pal=1 n=2
copy id=42 asked=0 used=0 frame=0
cache id=110 conv=0 ret=0 pal=0
info i=0 op=0 a=65536 inv=0 blend=0 bc=0,0,0,0 ret=0
blit x=0 y=0 w=1 h=1 rot=0
cache id=113 conv=0 ret=0 pal=0
info i=1 op=8 a=65536 inv=0 blend=0 bc=0,0,0,0 ret=0
blit x=0 y=0 w=1 h=1 rot=0
retv -1
bmp 04090eff
make id=43 w=1 h=1 cx=0 tx=0 pal=1 n=2
copy id=43 asked=0 used=0 frame=0
cache id=110 conv=0 ret=0 pal=0
info i=0 op=0 a=65536 inv=0 blend=0 bc=0,0,0,0 ret=0
blit x=0 y=0 w=1 h=1 rot=0
cache id=112 conv=0 ret=0 pal=0
info i=1 op=9 a=65536 inv=0 blend=0 bc=0,0,0,0 ret=0
blit x=0 y=0 w=1 h=1 rot=0
retv -1
bmp 09090900
make id=44 w=1 h=1 cx=0 tx=0 pal=1 n=2
copy id=44 asked=0 used=0 frame=0
cache id=110 conv=0 ret=0 pal=0
info i=0 op=0 a=65536 inv=0 blend=0 bc=0,0,0,0 ret=0
blit x=0 y=0 w=1 h=1 rot=0
cache id=112 conv=0 ret=0 pal=0
info i=1 op=0 a=65536 inv=0 blend=0 bc=0,0,0,0 ret=0
blit x=0 y=0 w=1 h=1 rot=0
retv -1
bmp 0a141eff
make id=45 w=1 h=1 cx=0 tx=0 pal=1 n=1
copy id=45 asked=0 used=0 frame=0
cache id=114 conv=0 ret=0 pal=0
info i=0 op=0 a=65536 inv=0 blend=1 bc=0,0,0,0 ret=0
blit x=0 y=0 w=1 h=1 rot=0
retv -1
bmp 241014ff
make id=46 w=1 h=1 cx=0 tx=0 pal=1 n=1
copy id=46 asked=0 used=0 frame=0
cache id=114 conv=0 ret=0 pal=0
info i=0 op=0 a=65536 inv=0 blend=32 bc=0,0,0,0 ret=0
blit x=0 y=0 w=1 h=1 rot=0
retv -1
bmp 232323ff
make id=47 w=1 h=1 cx=0 tx=0 pal=1 n=1
copy id=47 asked=0 used=0 frame=0
cache id=114 conv=0 ret=0 pal=0
info i=0 op=0 a=65536 inv=0 blend=33 bc=0,0,0,0 ret=0
blit x=0 y=0 w=1 h=1 rot=0
retv -1
bmp dc0123ff
make id=48 w=1 h=1 cx=0 tx=0 pal=1 n=1
copy id=48 asked=0 used=0 frame=0
cache id=114 conv=0 ret=0 pal=0
info i=0 op=0 a=65536 inv=0 blend=36 bc=0,0,0,0 ret=0
blit x=0 y=0 w=1 h=1 rot=0
retv -1
bmp dc0423ff
make id=49 w=1 h=1 cx=0 tx=0 pal=1 n=1
copy id=49 asked=0 used=0 frame=0
cache id=114 conv=0 ret=0 pal=0
info i=0 op=0 a=65536 inv=0 blend=40 bc=0,0,0,0 ret=0
blit x=0 y=0 w=1 h=1 rot=0
special oob 40
retv -1
bmp 00000000
make id=50 w=1 h=1 cx=0 tx=0 pal=1 n=1
copy id=50 asked=0 used=0 frame=0
cache id=110 conv=0 ret=0 pal=0
info i=0 op=0 a=65536 inv=0 blend=-1 bc=65536,32896,0,0 ret=0
blit x=0 y=0 w=1 h=1 rot=0
retv -1
bmp 000a1eff
make id=51 w=1 h=1 cx=0 tx=0 pal=1 n=1
copy id=51 asked=0 used=0 frame=0
cache id=110 conv=0 ret=0 pal=0
info i=0 op=0 a=65536 inv=0 blend=-2 bc=328960,657920,986880,32640 ret=0
blit x=0 y=0 w=1 h=1 rot=0
retv -1
bmp 141413ff
make id=52 w=2 h=1 cx=0 tx=0 pal=1 n=1
copy id=52 asked=0 used=0 frame=0
cache id=115 conv=0 ret=0 pal=0
info i=0 op=0 a=65536 inv=0 blend=0 bc=0,0,0,0 ret=0
blit x=0 y=0 w=2 h=1 rot=4
retv -1
bmp 040506ff010203ff
make id=53 w=1 h=1 cx=0 tx=0 pal=1 n=1
copy id=53 asked=0 used=0 frame=0
cache id=110 conv=0 ret=0 pal=0
info i=0 op=0 a=65536 inv=0 blend=0 bc=0,0,0,0 ret=0
clip miss x=3 y=0
retv -1
bmp 00000000
make id=54 w=1 h=1 cx=0 tx=0 pal=1 n=3
copy id=54 asked=0 used=0 frame=4
cache id=117 conv=0 ret=-1 pal=0
info i=0 op=0 a=65536 inv=0 blend=0 bc=0,0,0,0 ret=-1
blit x=0 y=0 w=1 h=1 rot=0
cache id=118 conv=0 ret=4 pal=0
info i=1 op=0 a=65536 inv=0 blend=0 bc=0,0,0,0 ret=4
blit x=0 y=0 w=1 h=1 rot=0
cache id=119 conv=0 ret=2 pal=0
info i=2 op=0 a=65536 inv=0 blend=0 bc=0,0,0,0 ret=2
blit x=0 y=0 w=1 h=1 rot=0
retv -1
bmp 030000ff
make id=55 w=1 h=1 cx=0 tx=0 pal=1 n=2
copy id=55 asked=0 used=0 frame=0
cache id=120 conv=0 ret=1 pal=0
info i=0 op=0 a=65536 inv=0 blend=0 bc=0,0,0,0 ret=1
blit x=0 y=0 w=1 h=1 rot=0
cache id=121 conv=0 ret=5 pal=0
info i=1 op=0 a=65536 inv=0 blend=0 bc=0,0,0,0 ret=5
blit x=0 y=0 w=1 h=1 rot=0
retv -1
bmp 050000ff
make id=56 w=1 h=1 cx=0 tx=0 pal=1 n=0
copy id=56 asked=0 used=0 frame=0
retv -1
bmp 00000000
make id=57 w=1 h=1 cx=0 tx=1 pal=1 n=1
copy id=57 asked=2 used=0 frame=9
cache id=110 conv=0 ret=0 pal=0
info i=0 op=0 a=65536 inv=0 blend=0 bc=0,0,0,0 ret=0
blit x=0 y=0 w=1 h=1 rot=0
retv -1
bmp 0a141eff
make id=58 w=1 h=1 cx=0 tx=0 pal=1 n=1
copy id=58 asked=2 used=2 frame=9
cache id=110 conv=2 ret=0 pal=0
info i=0 op=0 a=65536 inv=0 blend=0 bc=0,0,0,0 ret=0
blit x=0 y=0 w=1 h=1 rot=0
retv -1
bmp 0a141eff
copy id=58 asked=1 used=1 frame=2
cache id=110 conv=1 ret=0 pal=0
info i=0 op=0 a=65536 inv=0 blend=0 bc=0,0,0,0 ret=0
blit x=0 y=0 w=1 h=1 rot=0
retv -1
bmp 0a141eff
make id=59 w=1 h=1 cx=0 tx=0 pal=1 n=1
copy id=59 asked=0 used=0 frame=0
cache id=134 conv=0 ret=0 pal=1
info i=0 op=0 a=65536 inv=0 blend=0 bc=0,0,0,0 ret=0
blit x=0 y=0 w=1 h=1 rot=0
retv -1
bmp 030201ff
make id=60 w=1 h=1 cx=0 tx=0 pal=1 n=1
copy id=60 asked=0 used=0 frame=0
cache id=135 conv=0 ret=0 pal=1
info i=0 op=0 a=65536 inv=0 blend=0 bc=0,0,0,0 ret=0
blit x=0 y=0 w=1 h=1 rot=0
retv -1
bmp 00000000
make id=61 w=1 h=1 cx=0 tx=0 pal=1 n=1
pre id=61 in=0 child=0
child 130 tc=0 pal=1
self tc=0 pal=1
make id=62 w=1 h=1 cx=0 tx=0 pal=1 n=1
pre id=62 in=0 child=1
child 130 tc=1 pal=1
self tc=0 pal=1
make id=63 w=1 h=1 cx=1 tx=0 pal=0 n=1
pre id=63 in=0 child=1
child 130 tc=2 pal=1
self tc=0 pal=1
pre id=61 in=1 child=1
child 130 tc=3 pal=1
self tc=1 pal=1
make id=64 w=1 h=1 cx=0 tx=0 pal=1 n=2
pre id=64 in=0 child=0
child 131 tc=0 pal=1
child 131 tc=0 pal=2
self tc=0 pal=1

""";
}
