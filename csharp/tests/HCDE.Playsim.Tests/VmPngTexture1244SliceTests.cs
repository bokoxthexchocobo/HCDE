using System.Text;
using HCDE.Playsim;

namespace HCDE.Playsim.Tests;

public class VmPngTexture1244SliceTests
{
    const string HeadGolden =
"""
palettize type=2 lum=0 n=4 out=3f000202
palettize type=2 lum=0 n=4 out=3f000002
palettize type=2 lum=1 n=4 out=4c240212
palettize type=6 lum=1 n=1 out=26
palettize type=6 lum=0 n=1 out=3f
palettize type=6 lum=0 n=1 out=00
palettize type=4 lum=0 n=4 out=0a1e0028
palettize type=4 lum=1 n=4 out=071e0014

""";

    const string TailGolden =
"""
---- edge
chunk PLTE len=9
chunk tRNS len=3
trns partial=1
stop IDAT
idat raw=04000000 be=4 id=IDAT pitch=2 decoded=4
clip w=2 h=2 ox=0 oy=0 sx=1 sy=2 at=0
pal op=0 0000ff8000ff00ff000000000000ff80
transpal=1
stop IDAT
idat raw=01000000 be=1 id=IDAT pitch=3 decoded=3
clip w=1 h=1 ox=0 oy=0 sx=3 sy=3 at=0
copy ct=1 rot=0 ok 00000000
transpal=1
stop IDAT
idat raw=01000000 be=1 id=IDAT pitch=2 decoded=2
clip w=1 h=1 ox=0 oy=0 sx=2 sy=2 at=0
copy ct=3 rot=0 ok 0a0a0ac8
transpal=-1
stop IDAT
gray a0 idx=1
idat raw=01000000 be=1 id=IDAT pitch=2 decoded=2
clip w=2 h=1 ox=0 oy=0 sx=1 sy=2 at=0
pal op=0 00000000000000ff
transpal=1
ihdr missing
reject c=0 f=0 i=1 d=8 t=2
reject c=0 f=0 i=0 d=8 t=6
file w=1 h=1 t=3 masked=0 translucent=0
bgra chunk PLTE len=3
bgra plte size=1
bgra stop IDAT
bgra idat at=48
bgra idat raw=01000000 be=1 id=IDAT n=1
clip w=1 h=1 ox=0 oy=0 sx=1 sy=1 at=0
pal op=0 0000ffff
setup called lines=10 map10=0
palettize type=4 lum=0 n=2 out=000b
cube missing
palettize type=2 lum=0 n=1 out=00

""";

    [Fact]
    public void Palettize_WalksColumnsAndGrayAlpha()
    {
        string dump = Build(out _, out _, out _, out _, out _);
        int cut = dump.IndexOf("---- edge\n", StringComparison.Ordinal);
        string head = dump[..cut];
        Assert.Contains("palettize type=2 lum=0 n=4 out=3f000202\n", head, StringComparison.Ordinal);
        Assert.Contains("palettize type=2 lum=0 n=4 out=3f000002\n", head, StringComparison.Ordinal);
        Assert.Contains("palettize type=2 lum=1 n=4 out=4c240212\n", head, StringComparison.Ordinal);
        Assert.Contains("palettize type=6 lum=1 n=1 out=26\n", head, StringComparison.Ordinal);
        Assert.Contains("palettize type=6 lum=0 n=1 out=3f\n", head, StringComparison.Ordinal);
        Assert.Contains("palettize type=6 lum=0 n=1 out=00\n", head, StringComparison.Ordinal);
        Assert.Contains("palettize type=4 lum=0 n=4 out=0a1e0028\n", head, StringComparison.Ordinal);
        Assert.Contains("palettize type=4 lum=1 n=4 out=071e0014\n", head, StringComparison.Ordinal);
        Assert.Equal(HeadGolden, head);
    }

    [Fact]
    public void Copy_BuildsBitmapAndSavegameTexture()
    {
        string dump = Build(out byte[] pal, out byte[] keyed, out byte[] ia, out byte[] gray, out byte[] bgra);
        string tail = dump[dump.IndexOf("---- edge\n", StringComparison.Ordinal)..];
        Assert.Contains("trns partial=1\n", tail, StringComparison.Ordinal);
        Assert.Contains("transpal=1\n", tail, StringComparison.Ordinal);
        Assert.Contains("transpal=-1\n", tail, StringComparison.Ordinal);
        Assert.Contains("gray a0 idx=1\n", tail, StringComparison.Ordinal);
        Assert.Contains("ihdr missing\n", tail, StringComparison.Ordinal);
        Assert.Contains("reject c=0 f=0 i=1 d=8 t=2\n", tail, StringComparison.Ordinal);
        Assert.Contains("reject c=0 f=0 i=0 d=8 t=6\n", tail, StringComparison.Ordinal);
        Assert.Contains("file w=1 h=1 t=3 masked=0 translucent=0\n", tail, StringComparison.Ordinal);
        Assert.Contains("setup called lines=", tail, StringComparison.Ordinal);
        Assert.Contains("map10=0\n", tail, StringComparison.Ordinal);
        Assert.Contains("cube missing\n", tail, StringComparison.Ordinal);
        Assert.Equal(new byte[] { 0x00, 0x00, 0xFF, 0x80, 0x00, 0xFF, 0x00, 0xFF, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xFF, 0x80 }, pal);
        Assert.Equal(new byte[] { 0x00, 0x00, 0x00, 0x00 }, keyed);
        Assert.Equal(new byte[] { 0x0A, 0x0A, 0x0A, 0xC8 }, ia);
        Assert.Equal(new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xFF }, gray);
        Assert.Equal(new byte[] { 0x00, 0x00, 0xFF, 0xFF }, bgra);
        Assert.Equal(TailGolden, tail);
    }

    static string Build(out byte[] pal, out byte[] keyed, out byte[] ia, out byte[] gray, out byte[] bgra)
    {
        var vm = new VmPngTexture1244 { Cube = (r, g, b) => r, Width = 2, Height = 2, ColorType = 2 };
        byte[] rgb =
        {
            255, 0, 0,
            8, 0, 0,
            0, 0, 255,
            10, 20, 30,
        };
        byte[] plain = vm.Palettize(rgb, 0);
        vm.HaveTrans = true;
        vm.NonPaletteTrans[0] = 8;
        byte[] keyedIdx = vm.Palettize(rgb, 0);
        vm.HaveTrans = false;
        byte[] luma = vm.Palettize(rgb, VmPngTexture1244.Luminance);
        vm.Width = 1;
        vm.Height = 1;
        vm.ColorType = 6;
        byte[] rgbaLum = vm.Palettize(new byte[] { 255, 0, 0, 128 }, VmPngTexture1244.Luminance);
        byte[] rgbaCube = vm.Palettize(new byte[] { 255, 0, 0, 128 }, 0);
        byte[] rgbaCut = vm.Palettize(new byte[] { 255, 0, 0, 100 }, 0);
        vm.Width = 2;
        vm.Height = 2;
        vm.ColorType = 4;
        vm.PaletteMap = Identity();
        byte[] ga =
        {
            10, 200,
            20, 10,
            30, 255,
            40, 128,
        };
        byte[] gaIdx = vm.Palettize(ga, 0);
        byte[] gaLum = vm.Palettize(ga, VmPngTexture1244.Luminance);

        vm.Log.Add("---- edge");

        vm.Width = 2;
        vm.Height = 2;
        vm.ColorType = 3;
        vm.PaletteSize = 3;
        vm.HaveTrans = false;
        byte[] palFile = Png(
            Chunk("PLTE", new byte[] { 255, 0, 0, 0, 255, 0, 0, 0, 255 }),
            Chunk("tRNS", new byte[] { 128, 255, 0 }),
            Chunk("IDAT", new byte[] { 1, 2, 3, 4 }));
        vm.StartOfIdat = (uint)(IndexOf(palFile, "IDAT") - 4);
        int palTrans = vm.CopyPixels(palFile, new byte[] { 0, 1, 2, 0 }, 0, 0);
        pal = (byte[])vm.LastBitmap!.Data.Clone();

        vm.Width = 1;
        vm.Height = 1;
        vm.ColorType = 2;
        vm.HaveTrans = true;
        vm.NonPaletteTrans[0] = 8;
        vm.NonPaletteTrans[1] = 0;
        vm.NonPaletteTrans[2] = 0;
        byte[] rgbFile = Png(Chunk("IDAT", new byte[] { 9 }));
        vm.StartOfIdat = (uint)(IndexOf(rgbFile, "IDAT") - 4);
        vm.CopyPixels(rgbFile, new byte[] { 8, 0, 0 }, 0, 0);
        keyed = (byte[])vm.LastBitmap!.Data.Clone();

        vm.ColorType = 4;
        vm.HaveTrans = false;
        byte[] iaFile = Png(Chunk("IDAT", new byte[] { 1 }));
        vm.StartOfIdat = (uint)(IndexOf(iaFile, "IDAT") - 4);
        vm.CopyPixels(iaFile, new byte[] { 10, 200 }, 0, 0);
        ia = (byte[])vm.LastBitmap!.Data.Clone();

        vm.Width = 2;
        vm.Height = 1;
        vm.ColorType = 0;
        vm.HaveTrans = true;
        vm.NonPaletteTrans[0] = 1;
        byte[] gFile = Png(Chunk("IDAT", new byte[] { 1 }));
        vm.StartOfIdat = (uint)(IndexOf(gFile, "IDAT") - 4);
        vm.CopyPixels(gFile, new byte[] { 1, 0 }, 0, 0);
        gray = (byte[])vm.LastBitmap!.Data.Clone();

        Assert.Null(vm.CreateFromFile(null, null, 0));
        Assert.Null(vm.CreateFromFile(null, Ihdr(2, 2, 8, 2, 0, 0, 1), 13));
        Assert.Null(vm.CreateFromFile(null, Ihdr(2, 2, 8, 6, 0, 0, 0), 13));
        byte[] save = Png(
            Chunk("PLTE", new byte[] { 255, 0, 0 }),
            Chunk("IDAT", new byte[] { 1 }));
        var pic = vm.CreateFromFile(save, Ihdr(1, 1, 8, 3, 0, 0, 0), 13);
        Assert.NotNull(pic);
        Assert.False(pic!.Masked);
        Assert.False(pic.Translucent);
        var surface = vm.GetBgra(pic, new byte[] { 0 }, null, null);
        bgra = (byte[])surface!.Data.Clone();

        vm.Width = 2;
        vm.Height = 1;
        vm.ColorType = 4;
        vm.PaletteMap = null;
        vm.SourceLump = 4;
        vm.Lumps[4] = new byte[3];
        var host = new VmPngTexture1243();
        host.ColorType = 0;
        host.HaveTrans = true;
        host.NonPaletteTrans[0] = 10;
        vm.PaletteHost = host;
        byte[] hosted = vm.Palettize(new byte[] { 10, 200, 11, 200 }, 0);

        vm.Cube = null;
        vm.Width = 1;
        vm.Height = 1;
        vm.ColorType = 2;
        vm.HaveTrans = false;
        vm.PaletteMap = Identity();
        byte[] missingCube = vm.Palettize(new byte[] { 255, 0, 0 }, 0);

        string dump = string.Join('\n', vm.Log) + "\n";
        Assert.Equal(new byte[] { 63, 0, 2, 2 }, plain);
        Assert.Equal(new byte[] { 63, 0, 0, 2 }, keyedIdx);
        Assert.Equal(new byte[] { 76, 36, 2, 18 }, luma);
        Assert.Equal(new byte[] { 38 }, rgbaLum);
        Assert.Equal(new byte[] { 63 }, rgbaCube);
        Assert.Equal(new byte[] { 0 }, rgbaCut);
        Assert.Equal(new byte[] { 10, 30, 0, 40 }, gaIdx);
        Assert.Equal(new byte[] { 7, 30, 0, 20 }, gaLum);
        Assert.Equal(new byte[] { 0, 11 }, hosted);
        Assert.Equal(new byte[] { 0 }, missingCube);
        Assert.Equal(1, palTrans);
        return dump;
    }

    static byte[] Identity()
    {
        var map = new byte[256];
        for (int i = 0; i < 256; i++)
            map[i] = (byte)i;
        return map;
    }

    static int IndexOf(byte[] file, string text)
    {
        byte[] key = Encoding.ASCII.GetBytes(text);
        for (int i = 0; i <= file.Length - key.Length; i++)
        {
            bool ok = true;
            for (int j = 0; j < key.Length; j++)
            {
                if (file[i + j] != key[j])
                {
                    ok = false;
                    break;
                }
            }
            if (ok)
                return i;
        }
        return -1;
    }

    static byte[] Ihdr(int w, int h, byte depth, byte kind, byte compression, byte filter, byte interlace)
    {
        var buf = new byte[13];
        WriteBe(buf, 0, w);
        WriteBe(buf, 4, h);
        buf[8] = depth;
        buf[9] = kind;
        buf[10] = compression;
        buf[11] = filter;
        buf[12] = interlace;
        return buf;
    }

    static void WriteBe(byte[] buf, int at, int v)
    {
        buf[at] = (byte)(v >> 24);
        buf[at + 1] = (byte)(v >> 16);
        buf[at + 2] = (byte)(v >> 8);
        buf[at + 3] = (byte)v;
    }

    static byte[] Be(uint v) => new[] { (byte)(v >> 24), (byte)(v >> 16), (byte)(v >> 8), (byte)v };

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
