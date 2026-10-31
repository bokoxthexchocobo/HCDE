using System.Text;
using HCDE.Playsim;

namespace HCDE.Playsim.Tests;

public class VmPngTexture1243SliceTests
{
    const string HeadGolden =
"""
chunk gAMA len=4
chunk tRNS len=2
trns npt=10,65535,65535
stop IDAT
idat at=63 tell=71
done mask=1 have=1 pal=256 idat=63 left=0 top=0 npt=10,65535,65535 gray=defer ref=0 depth=8 type=0 ilace=0 w=2 h=2 lump=1
setup chunk gAMA len=4
setup chunk tRNS len=2
setup stop IDAT
setup idat at=63 tell=71
setup gray copy zero=10 copy=0 shared=10 ref=0
setup back=5
pix len=04000000 id=IDAT alpha=0 n=4 undecoded=1
pix len=04000000 id=IDAT alpha=1 n=4 undecoded=1
chunk grAb len=8
grab x=5 y=-1
chunk PLTE len=18
plte size=6 at=61
chunk tRNS len=6
trns npt=65535,65280,65535
stop IDAT
idat at=101 tell=109
mask idx=3
done mask=1 have=1 pal=6 idat=101 left=5 top=-1 npt=65535,65280,65535 gray=none ref=0 depth=8 type=3 ilace=0 w=2 h=2 lump=2
setup chunk grAb len=8
setup chunk PLTE len=18
setup chunk tRNS len=6
setup stop IDAT
setup idat at=101 tell=109
setup remap exact 5->255
setup remap exact 1->10
setup remap near 4->1
setup remap near 2->4
setup remap near 0->10
setup force0 3
setup map=0a0a040001ff
setup back=9
alpha i=0 map=10 out=76
alpha i=1 map=10 out=76
alpha i=2 map=4 out=3
alpha i=3 map=0 out=0
alpha i=4 map=1 out=1
alpha i=5 map=255 out=1
alpha back=4
pix len=04000000 id=IDAT alpha=0 n=4 undecoded=1
stop IDAT
idat at=33 tell=41
done mask=1 have=0 pal=0 idat=33 left=0 top=0 npt=unset gray=shared ref=1 depth=8 type=4 ilace=0 w=2 h=2 lump=3
chunk tRNS len=6
trns npt=1,2,3
stop IDAT
idat at=51 tell=59
done mask=1 have=1 pal=0 idat=51 left=0 top=0 npt=1,2,3 gray=none ref=0 depth=8 type=2 ilace=0 w=2 h=2 lump=4
stop IDAT
idat at=33 tell=41
done mask=1 have=0 pal=0 idat=33 left=0 top=0 npt=unset gray=none ref=0 depth=8 type=6 ilace=1 w=2 h=2 lump=5

""";

    const string TailGolden =
"""
---- edge
seek fail to=33
header short len=3 id=0
chunk 00000000 len=0
seek fail to=7
cont short len=0 id=0
stop IEND
idat at=4294967291 tell=3
done mask=0 have=0 pal=0 idat=4294967291 left=0 top=0 npt=unset gray=none ref=0 depth=8 type=2 ilace=0 w=2 h=2 lump=6
chunk grAb len=8
grab bad x=65536
grab x=0 y=-1
stop IEND
idat at=53 tell=61
done mask=0 have=0 pal=0 idat=53 left=0 top=-1 npt=unset gray=none ref=0 depth=8 type=2 ilace=0 w=2 h=2 lump=7
pix len=00000000 id=IEND alpha=0 n=4 undecoded=1
pix n=4 fill=99
chunk tRNS len=300
trns cap
trns npt=1,2,3
stop IDAT
idat at=345 tell=353
done mask=1 have=1 pal=0 idat=345 left=0 top=0 npt=1,2,3 gray=none ref=0 depth=8 type=2 ilace=0 w=2 h=2 lump=8
stop IDAT
idat at=33 tell=41
done mask=0 have=0 pal=0 idat=33 left=0 top=0 npt=unset gray=none ref=0 depth=8 type=3 ilace=0 w=2 h=2 lump=9
setup stop IDAT
setup idat at=33 tell=41
setup plte absent
setup back=12
chunk PLTE len=3
plte size=1 at=41
chunk tRNS len=2
trns npt=10,65535,65535
stop IDAT
idat at=62 tell=70
done mask=1 have=1 pal=256 idat=62 left=0 top=0 npt=10,65535,65535 gray=defer ref=0 depth=8 type=0 ilace=0 w=2 h=2 lump=10
pix len=04000000 id=IDAT alpha=0 n=4 undecoded=1
setup chunk PLTE len=3
setup plte short 37/768
setup seek fail to=4294966609
setup seek fail to=82
setup cont short len=0 id=0
setup stop IEND
setup idat at=70 tell=78
setup gray copy zero=10 copy=0 shared=10 ref=0
setup back=0
chunk tRNS len=6
trns npt=258,772,1286
chunk tRNS len=2
trns npt=10,772,1286
stop IDAT
idat at=65 tell=73
done mask=1 have=1 pal=0 idat=65 left=0 top=0 npt=10,772,1286 gray=none ref=0 depth=8 type=2 ilace=0 w=2 h=2 lump=11
chunk tRNS len=2
trns npt=256,65535,65535
stop IDAT
idat at=47 tell=55
done mask=0 have=1 pal=0 idat=47 left=0 top=0 npt=256,65535,65535 gray=shared ref=1 depth=8 type=0 ilace=0 w=2 h=2 lump=12
chunk grAb len=8
grab x=1 y=2
chunk grAb len=8
grab x=9 y=8
stop IEND
idat at=73 tell=81
done mask=1 have=0 pal=0 idat=73 left=9 top=8 npt=unset gray=none ref=0 depth=8 type=6 ilace=0 w=2 h=2 lump=13
alpha i=0 map=0 out=0
alpha i=1 map=1 out=142
alpha back=4
alpha map oob i=0
alpha i=0 map=0 out=0
alpha back=0
alpha short i=0
alpha i=0 map=5 out=0
alpha back=0
alpha seek fail to=100
alpha i=0 map=5 out=76
alpha back=0
lump missing
pix n=4 fill=99
lump missing
pix seek fail to=8
pix header short len=0 id=0
pix len=00000000 id=00000000 alpha=1 n=4 undecoded=1

""";

    [Fact]
    public void Walk_DefersGrayAndRemapsPalette()
    {
        string dump = Build();
        int cut = dump.IndexOf("---- edge\n", StringComparison.Ordinal);
        string head = dump[..cut];
        Assert.Contains("gray=defer", head, StringComparison.Ordinal);
        Assert.Contains("setup gray copy zero=10 copy=0 shared=10 ref=0\n", head, StringComparison.Ordinal);
        Assert.Contains("chunk gAMA len=4\n", head, StringComparison.Ordinal);
        Assert.Contains("grab x=5 y=-1\n", head, StringComparison.Ordinal);
        Assert.Contains("mask idx=3\n", head, StringComparison.Ordinal);
        Assert.Contains("setup remap exact 5->255\n", head, StringComparison.Ordinal);
        Assert.Contains("setup remap exact 1->10\n", head, StringComparison.Ordinal);
        Assert.Contains("setup remap near 4->1\n", head, StringComparison.Ordinal);
        Assert.Contains("setup remap near 2->4\n", head, StringComparison.Ordinal);
        Assert.Contains("setup remap near 0->10\n", head, StringComparison.Ordinal);
        Assert.Contains("setup force0 3\n", head, StringComparison.Ordinal);
        Assert.Contains("setup map=0a0a040001ff\n", head, StringComparison.Ordinal);
        Assert.Contains("alpha i=0 map=10 out=76\n", head, StringComparison.Ordinal);
        Assert.Contains("alpha i=3 map=0 out=0\n", head, StringComparison.Ordinal);
        Assert.Contains("alpha i=5 map=255 out=1\n", head, StringComparison.Ordinal);
        Assert.Contains("alpha back=4\n", head, StringComparison.Ordinal);
        Assert.Contains("pix len=04000000 id=IDAT alpha=0 n=4 undecoded=1\n", head, StringComparison.Ordinal);
        Assert.Contains("pix len=04000000 id=IDAT alpha=1 n=4 undecoded=1\n", head, StringComparison.Ordinal);
        Assert.Contains("gray=shared ref=1 ", head, StringComparison.Ordinal);
        Assert.Contains("npt=1,2,3 gray=none ", head, StringComparison.Ordinal);
        Assert.Equal(HeadGolden, head);
    }

    [Fact]
    public void Edge_CapsShortReadsAndLeavesIdatUndecoded()
    {
        string dump = Build();
        string tail = dump[dump.IndexOf("---- edge\n", StringComparison.Ordinal)..];
        Assert.Contains("header short len=3 id=0\n", tail, StringComparison.Ordinal);
        Assert.Contains("idat at=4294967291 tell=3\n", tail, StringComparison.Ordinal);
        Assert.Contains("grab bad x=65536\n", tail, StringComparison.Ordinal);
        Assert.Contains("grab x=0 y=-1\n", tail, StringComparison.Ordinal);
        Assert.Contains("pix len=00000000 id=IEND alpha=0 n=4 undecoded=1\n", tail, StringComparison.Ordinal);
        Assert.Contains("pix n=4 fill=99\n", tail, StringComparison.Ordinal);
        Assert.Contains("trns cap\n", tail, StringComparison.Ordinal);
        Assert.Contains("setup plte absent\n", tail, StringComparison.Ordinal);
        Assert.Contains("setup back=12\n", tail, StringComparison.Ordinal);
        Assert.Contains("plte short ", tail, StringComparison.Ordinal);
        Assert.Contains("seek fail to=", tail, StringComparison.Ordinal);
        Assert.Contains("npt=10,772,1286\n", tail, StringComparison.Ordinal);
        Assert.Contains("npt=256,65535,65535 gray=shared ", tail, StringComparison.Ordinal);
        Assert.Contains("alpha i=0 map=0 out=0\n", tail, StringComparison.Ordinal);
        Assert.Contains("alpha i=1 map=1 out=142\n", tail, StringComparison.Ordinal);
        Assert.Contains("alpha map oob i=0\n", tail, StringComparison.Ordinal);
        Assert.Contains("alpha short i=0\n", tail, StringComparison.Ordinal);
        Assert.Contains("lump missing\n", tail, StringComparison.Ordinal);
        Assert.Contains("grab x=9 y=8\n", tail, StringComparison.Ordinal);
        Assert.Equal(TailGolden, tail);
    }

    static string Build()
    {
        var vm = new VmPngTexture1243();
        PaintBase(vm.BaseColors);

        byte[] gray = Png(
            Chunk("gAMA", Be(1)),
            Chunk("tRNS", new byte[] { 0x00, 0x0A }),
            Chunk("IDAT", new byte[] { 0xDE, 0xAD, 0xBE, 0xEF }));
        vm.Lumps[1] = gray;
        vm.Construct(gray, 1, 2, 2, 8, 0, 0);
        vm.SetupPalette(gray, 5);
        byte[]? grayCopy = vm.PaletteMap;
        byte[] pix0 = vm.CreatePalettedPixels(0, 7);
        byte[] pix1 = vm.CreatePalettedPixels(VmPngTexture1243.Luminance, 7);

        byte[] palette = Png(
            Chunk("grAb", Join(BeI(5), BeI(-1))),
            Chunk("PLTE", new byte[]
            {
                255, 0, 0,
                255, 0, 0,
                12, 0, 0,
                0, 0, 0,
                1, 2, 3,
                1, 2, 3,
            }),
            Chunk("tRNS", new byte[] { 255, 255, 255, 0, 255, 255 }),
            Chunk("IDAT", new byte[] { 1, 2, 3, 4 }));
        vm.Lumps[2] = palette;
        vm.Construct(palette, 2, 2, 2, 8, 3, 0);
        int left = vm.LeftOffset;
        int top = vm.TopOffset;
        vm.SetupPalette(palette, 9);
        byte[]? palMap = vm.PaletteMap;
        var alpha = Enumerable.Repeat((byte)0xEE, 8).ToArray();
        vm.ReadAlphaRemap(palette, alpha, 4);
        byte[] palPix = vm.CreatePalettedPixels(0, 7);

        byte[] type4 = Png(Chunk("IDAT", new byte[] { 9 }));
        vm.Lumps[3] = type4;
        vm.Construct(type4, 3, 2, 2, 8, 4, 0);
        bool type4Shared = ReferenceEquals(vm.PaletteMap, vm.GrayMap);

        byte[] type2 = Png(
            Chunk("tRNS", new byte[] { 0, 1, 0, 2, 0, 3 }),
            Chunk("IDAT", new byte[] { 1 }));
        vm.Lumps[4] = type2;
        vm.Construct(type2, 4, 2, 2, 8, 2, 0);

        byte[] type6 = Png(Chunk("IDAT", new byte[] { 2 }));
        vm.Lumps[5] = type6;
        vm.Construct(type6, 5, 2, 2, 8, 6, 1);

        vm.Log.Add("---- edge");

        vm.Construct(new byte[3], 6, 2, 2, 8, 2, 0);

        byte[] bad = Png(
            Chunk("grAb", Join(Be(0x00010000), Be(0xFFFFFFFFu))),
            Chunk("IEND", Array.Empty<byte>()));
        vm.Lumps[7] = bad;
        vm.Construct(bad, 7, 2, 2, 8, 2, 0);
        int badLeft = vm.LeftOffset;
        int badTop = vm.TopOffset;
        byte[] iendPix = vm.CreatePalettedPixels(0, 0);
        vm.StartOfIdat = 0;
        byte[] fill = vm.CreatePalettedPixels(0, 0);

        var wide = new byte[300];
        wide[0] = 0;
        wide[1] = 1;
        wide[2] = 0;
        wide[3] = 2;
        wide[4] = 0;
        wide[5] = 3;
        for (int i = 6; i < wide.Length; i++)
            wide[i] = 0xAB;
        byte[] capped = Png(Chunk("tRNS", wide), Chunk("IDAT", new byte[] { 1 }));
        vm.Lumps[8] = capped;
        vm.Construct(capped, 8, 2, 2, 8, 2, 0);

        byte[] bare = Png(Chunk("IDAT", new byte[] { 1, 2, 3, 4 }));
        vm.Lumps[9] = bare;
        vm.Construct(bare, 9, 2, 2, 8, 3, 0);
        vm.SetupPalette(bare, 12);

        byte[] shifted = Png(
            Chunk("PLTE", new byte[] { 255, 0, 0 }),
            Chunk("tRNS", new byte[] { 0x00, 0x0A }),
            Chunk("IDAT", new byte[] { 9, 9, 9, 9 }));
        vm.Lumps[10] = shifted;
        vm.Construct(shifted, 10, 2, 2, 8, 0, 0);
        byte[] shiftedPix = vm.CreatePalettedPixels(0, 0);
        vm.SetupPalette(shifted, 0);

        byte[] twice = Png(
            Chunk("tRNS", new byte[] { 1, 2, 3, 4, 5, 6 }),
            Chunk("tRNS", new byte[] { 0x00, 0x0A }),
            Chunk("IDAT", new byte[] { 1 }));
        vm.Construct(twice, 11, 2, 2, 8, 2, 0);

        byte[] high = Png(
            Chunk("tRNS", new byte[] { 0x01, 0x00 }),
            Chunk("IDAT", new byte[] { 1 }));
        vm.Construct(high, 12, 2, 2, 8, 0, 0);

        byte[] grabs = Png(
            Chunk("grAb", Join(BeI(1), BeI(2))),
            Chunk("grAb", Join(BeI(9), BeI(8))),
            Chunk("IEND", Array.Empty<byte>()));
        vm.Construct(grabs, 13, 2, 2, 8, 6, 0);
        int grabLeft = vm.LeftOffset;
        int grabTop = vm.TopOffset;

        var rgb = new byte[] { 255, 0, 0, 0, 255, 0 };
        vm.PaletteSize = 2;
        vm.PaletteMap = null;
        vm.StartOfPalette = 0;
        var nullAlpha = Enumerable.Repeat((byte)0xEE, 4).ToArray();
        vm.ReadAlphaRemap(rgb, nullAlpha, 4);

        vm.PaletteMap = Array.Empty<byte>();
        vm.PaletteSize = 1;
        vm.StartOfPalette = 0;
        var oobAlpha = Enumerable.Repeat((byte)0xEE, 2).ToArray();
        vm.ReadAlphaRemap(rgb, oobAlpha, 0);

        vm.PaletteMap = new byte[] { 5 };
        vm.PaletteSize = 1;
        vm.StartOfPalette = (uint)rgb.Length;
        var shortAlpha = Enumerable.Repeat((byte)0xEE, 2).ToArray();
        vm.ReadAlphaRemap(rgb, shortAlpha, 0);

        vm.PaletteMap = new byte[] { 5 };
        vm.PaletteSize = 1;
        vm.StartOfPalette = 100;
        var missAlpha = Enumerable.Repeat((byte)0xEE, 2).ToArray();
        vm.ReadAlphaRemap(rgb, missAlpha, 0);

        vm.SourceLump = 99;
        vm.StartOfIdat = 0;
        vm.Width = 2;
        vm.Height = 2;
        byte[] missingFill = vm.CreatePalettedPixels(0, 3);
        vm.StartOfIdat = 8;
        byte[] missingRead = vm.CreatePalettedPixels(VmPngTexture1243.Luminance, 3);

        string dump = string.Join('\n', vm.Log) + "\n";

        Assert.NotNull(grayCopy);
        Assert.Equal(0, grayCopy![10]);
        Assert.Equal(10, vm.GrayMap[10]);
        Assert.False(ReferenceEquals(grayCopy, vm.GrayMap));
        Assert.Equal(new byte[] { 0, 0, 0, 0 }, pix0);
        Assert.Equal(new byte[] { 0, 0, 0, 0 }, pix1);
        Assert.Equal(5, left);
        Assert.Equal(-1, top);
        Assert.Equal(new byte[] { 10, 10, 4, 0, 1, 255 }, palMap);
        Assert.Equal(new byte[] { 76, 76, 3, 0, 1, 1, 0xEE, 0xEE }, alpha);
        Assert.Equal(new byte[] { 0, 0, 0, 0 }, palPix);
        Assert.True(type4Shared);
        Assert.Equal(10, vm.GrayMap[10]);
        Assert.Equal(0, badLeft);
        Assert.Equal(-1, badTop);
        Assert.Equal(new byte[] { 0, 0, 0, 0 }, iendPix);
        Assert.Equal(new byte[] { 0x99, 0x99, 0x99, 0x99 }, fill);
        Assert.Equal(new byte[] { 0, 0, 0, 0 }, shiftedPix);
        Assert.Equal(9, grabLeft);
        Assert.Equal(8, grabTop);
        Assert.Equal(new byte[] { 0, 142, 0xEE, 0xEE }, nullAlpha);
        Assert.Equal(new byte[] { 0, 0xEE }, oobAlpha);
        Assert.Equal(new byte[] { 0, 0xEE }, shortAlpha);
        Assert.Equal(new byte[] { 76, 0xEE }, missAlpha);
        Assert.Equal(new byte[] { 0x99, 0x99, 0x99, 0x99 }, missingFill);
        Assert.Equal(new byte[] { 0, 0, 0, 0 }, missingRead);
        return dump;
    }

    static void PaintBase(uint[] pal)
    {
        for (int i = 0; i < pal.Length; i++)
            pal[i] = 0x0000FFu;
        pal[1] = 0x000000u;
        pal[4] = 0x0A0000u;
        pal[10] = 0xFF0000u;
        pal[255] = 0x010203u;
    }

    static byte[] Be(uint v) => new[] { (byte)(v >> 24), (byte)(v >> 16), (byte)(v >> 8), (byte)v };

    static byte[] BeI(int v) => Be(unchecked((uint)v));

    static byte[] Join(byte[] a, byte[] b)
    {
        var buf = new byte[a.Length + b.Length];
        Buffer.BlockCopy(a, 0, buf, 0, a.Length);
        Buffer.BlockCopy(b, 0, buf, a.Length, b.Length);
        return buf;
    }

    static byte[] Chunk(string type, byte[] data)
    {
        var buf = new List<byte>();
        buf.AddRange(Be((uint)data.Length));
        buf.AddRange(Encoding.ASCII.GetBytes(type));
        buf.AddRange(data);
        buf.AddRange(Be(0));
        return buf.ToArray();
    }

    static byte[] Png(params byte[][] parts)
    {
        var buf = new List<byte>(new byte[33]);
        foreach (byte[] part in parts)
            buf.AddRange(part);
        return buf.ToArray();
    }
}
