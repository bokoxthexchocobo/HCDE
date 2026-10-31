using HCDE.Playsim;

namespace HCDE.Playsim.Tests;

public class VmPatchTexture1237SliceTests
{
    [Fact]
    public void Probe_RejectsTheGapAndDrawsClippedPosts()
    {
        string dump = Build();
        int cut = dump.IndexOf("---- edge\n", StringComparison.Ordinal);
        string head = dump[..cut];
        Assert.Contains("probe gap\n", head, StringComparison.Ordinal);
        Assert.Contains("probe end x=0 ofs=100\n", head, StringComparison.Ordinal);
        Assert.Contains("probe oob x=1\n", head, StringComparison.Ordinal);
        Assert.Contains("probe end x=1 ofs=20\n", head, StringComparison.Ordinal);
        Assert.Contains("alpha 0\n", head, StringComparison.Ordinal);
        Assert.DoesNotContain("alpha 1", head, StringComparison.Ordinal);
        Assert.Contains("map=sky\n", head, StringComparison.Ordinal);
        Assert.Contains("map=gray\n", head, StringComparison.Ordinal);
        Assert.Contains("copy base asked=1 used=0 frame=1\n", head, StringComparison.Ordinal);
        Assert.Contains("draw oob x=0\n", head, StringComparison.Ordinal);
        Assert.Contains("spans 5\n", head, StringComparison.Ordinal);
        Assert.Equal(HeadGolden, head);
    }

    [Fact]
    public void BadScan_ZerosOffsetsAndPaintsRawColumns()
    {
        string dump = Build();
        string tail = dump[dump.IndexOf("---- edge\n", StringComparison.Ordinal)..];
        Assert.Contains("bad post x=0 top=0 len=1\n", tail, StringComparison.Ordinal);
        Assert.Contains("bad extra x=0 top=16\n", tail, StringComparison.Ordinal);
        Assert.Contains("bad early len=100 need=512\n", tail, StringComparison.Ordinal);
        Assert.Contains("bad oob x=0\n", tail, StringComparison.Ordinal);
        Assert.Contains("bad short\n", tail, StringComparison.Ordinal);
        Assert.Contains("remap0 0\n", tail, StringComparison.Ordinal);
        Assert.Contains("draw oob x=0\n", tail, StringComparison.Ordinal);
        Assert.DoesNotContain("spans ", tail, StringComparison.Ordinal);
        Assert.Equal(TailGolden, tail);
    }

    static string Build()
    {
        var vm = new VmPatchTexture1237();
        var log = vm.Log;

        vm.TryCreate(99);

        vm.Files[1] = Array.Empty<byte>();
        vm.TryCreate(1);

        vm.Files[2] = new byte[] { 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff };
        vm.TryCreate(2);

        vm.Files[3] = Lump(1, 1, 0, 0, new uint[] { 0 }, 12);
        vm.TryCreate(3);

        vm.Files[4] = Lump(1, 1, 0, 0, new uint[] { 0 }, 16);
        vm.TryCreate(4);

        vm.Files[5] = Lump(1, 1, 0, 0, new uint[] { 100 }, 16);
        vm.TryCreate(5);

        vm.Files[6] = Lump(2, 1, 0, 0, new uint[] { 0 }, 13);
        vm.TryCreate(6);

        vm.Files[7] = Lump(2, 1, 0, 0, new uint[] { 16, 20 }, 20);
        vm.TryCreate(7);

        vm.Files[8] = Lump(1, 1, 4096, 0, new uint[] { 12 }, 20);
        vm.TryCreate(8);

        vm.Files[9] = Lump(1, 1, -4096, 0, new uint[] { 12 }, 20);
        vm.TryCreate(9);

        vm.Files[10] = Lump(1, 0, 0, 0, new uint[] { 12 }, 20);
        vm.TryCreate(10);

        vm.Files[11] = Lump(0, 1, 0, 0, new uint[] { 12 }, 20);
        vm.TryCreate(11);

        vm.Files[12] = Lump(2049, 1, 0, 0, new uint[] { 12 }, 20);
        vm.TryCreate(12);

        vm.Files[13] = Lump(1, 2049, 0, 0, new uint[] { 12 }, 20);
        vm.TryCreate(13);

        vm.Files[14] = Lump(1, 1, 4095, -4095, new uint[] { 12 }, 18);
        var edge = vm.TryCreate(14);
        vm.Pixels(edge!, 0, 0);

        var posts = new byte[48];
        PutI16(posts, 0, 2);
        PutI16(posts, 2, 5);
        PutI16(posts, 4, 3);
        PutI16(posts, 6, -2);
        PutU32(posts, 8, 16);
        PutU32(posts, 12, 33);
        byte[] cols =
        {
            0, 0, 0, 0,
            1, 3, 0, 0, 2, 3, 0,
            1, 1, 0, 6, 0,
            255,
            4, 4, 0, 9, 8, 7, 6, 0,
            1, 2, 0, 5, 5, 0,
            255
        };
        Buffer.BlockCopy(cols, 0, posts, 16, cols.Length);
        vm.Files[15] = posts;
        var img = vm.TryCreate(15);
        vm.Pixels(img!, VmPatchTexture1237.Normal, 0);
        vm.Pixels(img!, VmPatchTexture1237.NoRemap0, 3);
        vm.Pixels(img!, VmPatchTexture1237.Luminance, 0);
        vm.Copy(img!, VmPatchTexture1237.Normal, 0);
        vm.Copy(img!, VmPatchTexture1237.Luminance, 1);
        vm.Remap0(img!);

        var alpha = vm.Make(15, 2, 5, 3, -2, true);
        vm.Pixels(alpha, VmPatchTexture1237.Normal, 0);
        vm.Pixels(alpha, VmPatchTexture1237.Luminance, 0);
        vm.Pixels(alpha, VmPatchTexture1237.NoRemap0, 0);
        vm.Copy(alpha, VmPatchTexture1237.Luminance, 2);
        vm.Remap0(alpha);

        vm.Files[16] = Lump(2, 1, 0, 0, new uint[] { 16, 15 }, 16);
        var ended = vm.TryCreate(16);
        vm.Pixels(ended!, 0, 0);

        var fence = Lump(1, 1, 0, 0, new uint[] { 12 }, 15);
        fence[12] = 2;
        fence[13] = 1;
        fence[14] = 9;
        vm.Files[17] = fence;
        var fenced = vm.TryCreate(17);
        vm.Pixels(fenced!, 0, 0);

        var clipped = Lump(1, 8, 0, 0, new uint[] { 12 }, 20);
        clipped[12] = 0;
        clipped[13] = 10;
        clipped[15] = 1;
        clipped[16] = 2;
        clipped[17] = 3;
        clipped[18] = 4;
        clipped[19] = 5;
        vm.Files[18] = clipped;
        var cut = vm.TryCreate(18);
        vm.Pixels(cut!, 0, 0);

        var wide = new byte[8200];
        PutI16(wide, 0, 2048);
        PutI16(wide, 2, 1);
        PutU32(wide, 8, 8200);
        vm.Files[19] = wide;
        vm.TryCreate(19);

        vm.Files[20] = Lump(1, 2048, 6, 7, new uint[] { 12 }, 13);
        vm.TryCreate(20);

        var big = new byte[10165];
        PutI16(big, 0, 1);
        PutI16(big, 2, 1);
        PutU32(big, 8, 12);
        vm.Files[21] = big;
        vm.TryCreate(21);

        log.Add("---- edge");

        vm.Files[22] = BadFile(1, 256, 9, 8, 4, 0xff);
        var bad = vm.TryCreate(22);
        vm.Pixels(bad!, 0, 0);
        vm.Remap0(bad!);
        vm.Copy(bad!, 0, 0);

        var near = new byte[200];
        PutI16(near, 0, 1);
        PutI16(near, 2, 256);
        PutI16(near, 4, 4);
        PutI16(near, 6, 5);
        PutU32(near, 8, 12);
        near[12] = 0;
        near[13] = 1;
        vm.Files[23] = near;
        vm.TryCreate(23);

        vm.Files[24] = BadFile(1, 256, 9, 8, 4, 0x10);
        vm.TryCreate(24);

        var zeroW = new byte[271];
        PutI16(zeroW, 0, 0);
        PutI16(zeroW, 2, 256);
        PutI16(zeroW, 4, 4);
        PutI16(zeroW, 6, 5);
        PutU32(zeroW, 8, 12);
        for (int i = 0; i < 256; i++)
            zeroW[15 + i] = 6;
        vm.Files[25] = zeroW;
        var hidden = vm.Make(25, 1, 256, 4, 5, false);
        vm.Pixels(hidden, 0, 0);

        var early = new byte[100];
        vm.Files[26] = early;
        vm.Make(26, 4, 256, 7, 8, false);

        var term = new byte[200];
        PutI16(term, 0, 1);
        PutI16(term, 2, 256);
        PutI16(term, 4, 2);
        PutI16(term, 6, 3);
        PutU32(term, 8, 190);
        vm.Files[27] = term;
        vm.Make(27, 1, 256, 2, 3, false);

        var plain = new byte[200];
        PutI16(plain, 0, 1);
        PutI16(plain, 2, 10);
        PutI16(plain, 4, 3);
        PutI16(plain, 6, 4);
        vm.Files[28] = plain;
        var kept = vm.Make(28, 1, 256, 3, 4, false);
        vm.Remap0(kept);

        vm.Make(29, 1, 1, 1, 2, false);

        vm.Files[30] = BadFile(1, 256, 8, 9, 4, 0xff);
        var tall = vm.Make(30, 1, 300, 8, 9, false);
        vm.Pixels(tall, 0, 0);

        return string.Join("\n", log) + "\n";
    }

    static byte[] BadFile(int w, int h, int lo, int to, byte pixel, byte term)
    {
        var d = Lump(w, h, lo, to, new uint[] { 12 }, 273);
        for (int i = 0; i < 256; i++)
            d[15 + i] = pixel;
        d[272] = term;
        return d;
    }

    static byte[] Lump(int w, int h, int lo, int to, uint[] columns, int size)
    {
        var d = new byte[size];
        PutI16(d, 0, w);
        PutI16(d, 2, h);
        PutI16(d, 4, lo);
        PutI16(d, 6, to);
        for (int i = 0; i < columns.Length && 8 + i * 4 + 4 <= d.Length; i++)
            PutU32(d, 8 + i * 4, columns[i]);
        return d;
    }

    static void PutI16(byte[] d, int at, int v)
    {
        d[at] = (byte)v;
        d[at + 1] = (byte)(v >> 8);
    }

    static void PutU32(byte[] d, int at, uint v)
    {
        d[at] = (byte)v;
        d[at + 1] = (byte)(v >> 8);
        d[at + 2] = (byte)(v >> 16);
        d[at + 3] = (byte)(v >> 24);
    }

    const string HeadGolden = """
missing 99
reject w=0 h=0 lo=0 to=0 len=0
reject w=65535 h=65535 lo=-1 to=255 len=7
probe short
miss
probe gap
miss
probe end x=0 ofs=100
miss
probe oob x=1
miss
probe end x=1 ofs=20
miss
reject w=1 h=1 lo=4096 to=0 len=20
reject w=1 h=1 lo=-4096 to=0 len=20
reject w=1 h=0 lo=0 to=0 len=20
reject w=0 h=1 lo=0 to=0 len=20
reject w=2049 h=1 lo=0 to=0 len=20
reject w=1 h=2049 lo=0 to=0 len=20
alpha 0
seek 8
create id=1 lump=14 w=1 h=1 lo=4095 to=-4095 alpha=0 pal=1 bad=0 mask=1
pixels conv=0 frame=0 bad=0 map=remap
spans 1
pix 00
alpha 0
seek 8
create id=2 lump=15 w=2 h=5 lo=3 to=-2 alpha=0 pal=1 bad=0 mask=1
pixels conv=0 frame=0 bad=0 map=remap
spans 5
pix 00070603000000000009
pixels conv=2 frame=3 bad=0 map=sky
spans 5
pix 00000603000000000009
pixels conv=1 frame=0 bad=0 map=gray
spans 5
pix 0022242100000000002b
copy base asked=0 used=0 frame=0
pixels conv=0 frame=0 bad=0 map=remap
spans 5
pix 00070603000000000009
bmp base 0000000000000000070809ff00000000060708ff00000000030405ff0000000000000000090a0bff
copy rc=0
copy base asked=1 used=0 frame=1
pixels conv=0 frame=1 bad=0 map=remap
spans 5
pix 00070603000000000009
bmp base 0000000000000000070809ff00000000060708ff00000000030405ff0000000000000000090a0bff
copy rc=0
remap0 1
create id=3 lump=15 w=2 h=5 lo=3 to=-2 alpha=1 pal=0 bad=0 mask=1
pixels conv=0 frame=0 bad=0 map=graymap
spans 5
pix 005a5c59000000000053
pixels conv=1 frame=0 bad=0 map=ramp
spans 5
pix 00111712000000000018
pixels conv=2 frame=0 bad=0 map=graymap
spans 5
pix 005a5c59000000000053
copy gray asked=1 frame=2
pixels conv=0 frame=2 bad=0 map=graymap
spans 5
pix 005a5c59000000000053
bmp gray ff0001ffff0001ffa55a01ffff0001ffa35c01ffff0001ffa65901ffff0001ffff0001ffac5301ff
copy rc=0
remap0 1
alpha 0
seek 8
create id=4 lump=16 w=2 h=1 lo=0 to=0 alpha=0 pal=1 bad=0 mask=1
pixels conv=0 frame=0 bad=0 map=remap
spans 2
pix 0000
alpha 0
seek 8
create id=5 lump=17 w=1 h=1 lo=0 to=0 alpha=0 pal=1 bad=0 mask=1
pixels conv=0 frame=0 bad=0 map=remap
spans 1
pix 00
alpha 0
seek 8
create id=6 lump=18 w=1 h=8 lo=0 to=0 alpha=0 pal=1 bad=0 mask=1
pixels conv=0 frame=0 bad=0 map=remap
draw oob x=0
spans 2
pix 0102030405000000
alpha 0
seek 8
create id=7 lump=19 w=2048 h=1 lo=0 to=0 alpha=0 pal=1 bad=0 mask=1
alpha 0
seek 8
bad early len=13 need=1024
create id=8 lump=20 w=1 h=2048 lo=6 to=7 alpha=0 pal=1 bad=0 mask=1
alpha 0
seek 8
create id=9 lump=21 w=1 h=1 lo=0 to=0 alpha=0 pal=1 bad=0 mask=1

""";

    const string TailGolden = """
---- edge
alpha 0
seek 8
create id=10 lump=22 w=1 h=256 lo=0 to=0 alpha=0 pal=1 bad=1 mask=0
pixels conv=0 frame=0 bad=1 map=remap
pix 04040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404
remap0 0
copy base asked=0 used=0 frame=0
pixels conv=0 frame=0 bad=1 map=remap
pix 04040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404
bmp base 040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff040506ff
copy rc=0
alpha 0
seek 8
bad post x=0 top=0 len=1
create id=11 lump=23 w=1 h=256 lo=4 to=5 alpha=0 pal=1 bad=0 mask=1
alpha 0
seek 8
bad extra x=0 top=16
create id=12 lump=24 w=1 h=256 lo=9 to=8 alpha=0 pal=1 bad=0 mask=1
create id=13 lump=25 w=1 h=256 lo=0 to=0 alpha=0 pal=1 bad=1 mask=0
pixels conv=0 frame=0 bad=1 map=remap
pix 06060606060606060606060606060606060606060606060606060606060606060606060606060606060606060606060606060606060606060606060606060606060606060606060606060606060606060606060606060606060606060606060606060606060606060606060606060606060606060606060606060606060606060606060606060606060606060606060606060606060606060606060606060606060606060606060606060606060606060606060606060606060606060606060606060606060606060606060606060606060606060606060606060606060606060606060606060606060606060606060606060606060606060606060606060606
bad early len=100 need=512
create id=14 lump=26 w=4 h=256 lo=7 to=8 alpha=0 pal=1 bad=0 mask=1
bad oob x=0
create id=15 lump=27 w=1 h=256 lo=2 to=3 alpha=0 pal=1 bad=0 mask=1
create id=16 lump=28 w=1 h=256 lo=3 to=4 alpha=0 pal=1 bad=0 mask=1
remap0 1
bad short
create id=17 lump=29 w=1 h=1 lo=1 to=2 alpha=0 pal=1 bad=0 mask=1
create id=18 lump=30 w=1 h=300 lo=0 to=0 alpha=0 pal=1 bad=1 mask=0
pixels conv=0 frame=0 bad=1 map=remap
draw oob x=0
pix 0404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040404040407ff000000000000000000000000000000000000000000000000000000000000000000000000000000000000

""";
}
