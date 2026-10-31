namespace HCDE.Playsim.Tests;

public class VmTgaTexture1245SliceTests
{
    const string HeadGolden =
"""
hdr type=1 bpp=8 cm=1 first=0 len=2 size=32 desc=40 id=0 hw=2 hh=1
pe zero
raw 2/2
flip origin=0 pitch=2 step=1
clip w=2 h=1 ox=0 oy=0 sx=1 sy=2 at=0
pal op=0 0000ff8000ff00ff
trans=1
hdr type=2 bpp=24 cm=0 first=0 len=0 size=0 desc=0 id=0 hw=1 hh=2
pe uninit
raw 6/6
flip origin=3 pitch=-3 step=3
clip w=1 h=2 ox=0 oy=0 sx=3 sy=-3 at=3
copy ct=7 rot=0 ok 040506ff010203ff
trans=0
hdr type=2 bpp=32 cm=0 first=0 len=0 size=0 desc=40 id=0 hw=1 hh=1
pe uninit
raw 4/4
flip origin=0 pitch=4 step=4
clip w=1 h=1 ox=0 oy=0 sx=4 sy=4 at=0
copy ct=8 rot=0 ok 0a141e28
trans=-1
hdr type=2 bpp=32 cm=0 first=0 len=0 size=0 desc=32 id=0 hw=1 hh=1
pe uninit
raw 4/4
flip origin=0 pitch=4 step=4
clip w=1 h=1 ox=0 oy=0 sx=4 sy=4 at=0
copy ct=7 rot=0 ok 0a141eff
trans=0
hdr type=3 bpp=8 cm=0 first=0 len=0 size=0 desc=32 id=0 hw=1 hh=1
pe uninit
raw 1/1
flip origin=0 pitch=1 step=1
clip w=1 h=1 ox=0 oy=0 sx=1 sy=1 at=0
pal op=0 070707ff
trans=0
hdr type=3 bpp=8 cm=1 first=0 len=1 size=32 desc=40 id=0 hw=1 hh=1
pe zero
raw 1/1
flip origin=0 pitch=1 step=1
clip w=1 h=1 ox=0 oy=0 sx=1 sy=1 at=0
pal op=0 000000ff
trans=1
hdr type=3 bpp=16 cm=0 first=0 len=0 size=0 desc=32 id=0 hw=1 hh=1
pe uninit
raw 2/2
flip origin=0 pitch=2 step=2
clip w=1 h=1 ox=0 oy=0 sx=2 sy=2 at=0
copy ct=9 rot=0 ok 222222ff
trans=0
hdr type=2 bpp=16 cm=0 first=0 len=0 size=0 desc=32 id=0 hw=1 hh=1
pe uninit
raw 2/2
flip origin=0 pitch=2 step=2
clip w=1 h=1 ox=0 oy=0 sx=2 sy=2 at=0
copy ct=10 rot=0 ok 0000f8ff
trans=0

""";

    const string TailGolden =
"""
---- edge
hdr type=9 bpp=8 cm=1 first=0 len=1 size=24 desc=32 id=0 hw=2 hh=1
pe zero
rle left=0 at=2
flip origin=0 pitch=2 step=1
clip w=2 h=1 ox=0 oy=0 sx=1 sy=2 at=0
pal op=0 0000ffff0000ffff
trans=0
hdr type=9 bpp=8 cm=1 first=0 len=1 size=24 desc=32 id=0 hw=2 hh=1
pe zero
rle left=0 at=2
flip origin=0 pitch=2 step=1
clip w=2 h=1 ox=0 oy=0 sx=1 sy=2 at=0
pal op=0 0000ffff0000ffff
trans=0
hdr type=10 bpp=24 cm=0 first=0 len=0 size=0 desc=32 id=0 hw=1 hh=1
pe uninit
rle left=0 at=3
flip origin=0 pitch=3 step=3
clip w=1 h=1 ox=0 oy=0 sx=3 sy=3 at=0
copy ct=7 rot=0 ok 090807ff
trans=0
lump missing
hdr short 0/18
hdr type=0 bpp=0 cm=0 first=0 len=0 size=0 desc=0 id=0 hw=0 hh=0
pe uninit
copy skip dim w=1 h=1 step=0 n=0 trans=0
hdr short 0/18
hdr type=0 bpp=0 cm=0 first=0 len=0 size=0 desc=0 id=0 hw=0 hh=0
pe uninit
copy skip dim w=1 h=1 step=0 n=0 trans=0
seek fail to=118
hdr type=1 bpp=8 cm=0 first=0 len=0 size=0 desc=32 id=100 hw=1 hh=1
pe uninit
raw 1/1
flip origin=0 pitch=1 step=1
clip w=1 h=1 ox=0 oy=0 sx=1 sy=1 at=0
pal op=0 5a5a5a5a
trans=0
hdr type=1 bpp=8 cm=1 first=300 len=1 size=24 desc=32 id=0 hw=1 hh=1
pe zero
raw 1/1
flip origin=0 pitch=1 step=1
clip w=1 h=1 ox=0 oy=0 sx=1 sy=1 at=0
pal op=0 5a5a5a5a
trans=0
hdr type=1 bpp=8 cm=1 first=0 len=1 size=15 desc=32 id=0 hw=1 hh=1
pe zero
raw 1/1
flip origin=0 pitch=1 step=1
clip w=1 h=1 ox=0 oy=0 sx=1 sy=1 at=0
pal op=0 f8f8f8ff
trans=0
hdr type=1 bpp=8 cm=1 first=0 len=1 size=24 desc=32 id=0 hw=1 hh=1
pe zero
raw 1/1
flip origin=0 pitch=1 step=1
clip w=1 h=1 ox=0 oy=0 sx=1 sy=1 at=0
pal op=0 010203ff
trans=0
hdr type=1 bpp=8 cm=1 first=0 len=1 size=0 desc=32 id=0 hw=1 hh=1
pe zero
cm default i=0
raw 1/1
flip origin=0 pitch=1 step=1
clip w=1 h=1 ox=0 oy=0 sx=1 sy=1 at=0
pal op=0 5a5a5a5a
trans=0
hdr type=2 bpp=8 cm=0 first=0 len=0 size=0 desc=32 id=0 hw=1 hh=1
pe uninit
raw 1/1
flip origin=0 pitch=1 step=1
rgb skip bpp=8
trans=0
hdr type=1 bpp=8 cm=1 first=-1 len=2 size=24 desc=32 id=0 hw=1 hh=1
pe zero
cm skip i=-1
raw 1/1
flip origin=0 pitch=1 step=1
clip w=1 h=1 ox=0 oy=0 sx=1 sy=1 at=0
pal op=0 010203ff
trans=0

""";

    [Fact]
    public void Copy_ExpandsPaletteRgbAndGray()
    {
        string dump = Build(out byte[] pal, out byte[] rgb, out byte[] bgra, out byte[] bare, out byte[] gray, out byte[] kept, out byte[] g16, out byte[] rgb16, out int palTrans, out int bgraTrans, out int keptTrans);
        int cut = dump.IndexOf("---- edge\n", StringComparison.Ordinal);
        string head = dump[..cut];
        Assert.Contains("trans=1\n", head, StringComparison.Ordinal);
        Assert.Contains("trans=-1\n", head, StringComparison.Ordinal);
        Assert.Contains("flip origin=3 pitch=-3 step=3\n", head, StringComparison.Ordinal);
        Assert.Contains("pe zero\n", head, StringComparison.Ordinal);
        Assert.Equal(new byte[] { 0x00, 0x00, 0xFF, 0x80, 0x00, 0xFF, 0x00, 0xFF }, pal);
        Assert.Equal(new byte[] { 0x04, 0x05, 0x06, 0xFF, 0x01, 0x02, 0x03, 0xFF }, rgb);
        Assert.Equal(new byte[] { 0x0A, 0x14, 0x1E, 0x28 }, bgra);
        Assert.Equal(new byte[] { 0x0A, 0x14, 0x1E, 0xFF }, bare);
        Assert.Equal(new byte[] { 0x07, 0x07, 0x07, 0xFF }, gray);
        Assert.Equal(new byte[] { 0x00, 0x00, 0x00, 0xFF }, kept);
        Assert.Equal(new byte[] { 0x22, 0x22, 0x22, 0xFF }, g16);
        Assert.Equal(new byte[] { 0x00, 0x00, 0xF8, 0xFF }, rgb16);
        Assert.Equal(1, palTrans);
        Assert.Equal(-1, bgraTrans);
        Assert.Equal(1, keptTrans);
        Assert.Equal(HeadGolden, head);
    }

    [Fact]
    public void Copy_ReadsRleAndRejectsShortFiles()
    {
        string dump = Build(out _, out _, out _, out _, out _, out _, out _, out _, out _, out _, out _);
        string tail = dump[dump.IndexOf("---- edge\n", StringComparison.Ordinal)..];
        Assert.Contains("rle left=0 at=2\n", tail, StringComparison.Ordinal);
        Assert.Contains("rle left=0 at=3\n", tail, StringComparison.Ordinal);
        Assert.Contains("hdr short 0/18\n", tail, StringComparison.Ordinal);
        Assert.Contains("seek fail to=118\n", tail, StringComparison.Ordinal);
        Assert.Contains("cm default i=0\n", tail, StringComparison.Ordinal);
        Assert.Contains("rgb skip bpp=8\n", tail, StringComparison.Ordinal);
        Assert.Contains("cm skip i=-1\n", tail, StringComparison.Ordinal);
        Assert.Contains("lump missing\n", tail, StringComparison.Ordinal);
        Assert.Equal(TailGolden, tail);
    }

    static string Build(
        out byte[] pal, out byte[] rgb, out byte[] bgra, out byte[] bare, out byte[] gray,
        out byte[] kept, out byte[] g16, out byte[] rgb16,
        out int palTrans, out int bgraTrans, out int keptTrans)
    {
        var vm = new VmTgaTexture1245 { Width = 2, Height = 1 };
        byte[] palFile = Lump(0, 1, 1, 0, 2, 32, 2, 1, 8, 40,
            new byte[] { 0, 0, 255, 128, 0, 255, 0, 255, 0, 1 });
        palTrans = vm.CopyPixels(palFile, 0, 0);
        pal = (byte[])vm.LastBitmap!.Data.Clone();

        vm.Width = 1;
        vm.Height = 2;
        byte[] rgbFile = Lump(0, 0, 2, 0, 0, 0, 1, 2, 24, 0, new byte[] { 1, 2, 3, 4, 5, 6 });
        vm.CopyPixels(rgbFile, 0, 0);
        rgb = (byte[])vm.LastBitmap!.Data.Clone();

        vm.Width = 1;
        vm.Height = 1;
        byte[] bgraFile = Lump(0, 0, 2, 0, 0, 0, 1, 1, 32, 40, new byte[] { 10, 20, 30, 40 });
        bgraTrans = vm.CopyPixels(bgraFile, 0, 0);
        bgra = (byte[])vm.LastBitmap!.Data.Clone();

        byte[] bareFile = Lump(0, 0, 2, 0, 0, 0, 1, 1, 32, 32, new byte[] { 10, 20, 30, 99 });
        vm.CopyPixels(bareFile, 0, 0);
        bare = (byte[])vm.LastBitmap!.Data.Clone();

        byte[] grayFile = Lump(0, 0, 3, 0, 0, 0, 1, 1, 8, 32, new byte[] { 7 });
        vm.CopyPixels(grayFile, 0, 0);
        gray = (byte[])vm.LastBitmap!.Data.Clone();

        byte[] keptFile = Lump(0, 1, 3, 0, 1, 32, 1, 1, 8, 40, new byte[] { 0, 0, 0, 128, 0 });
        keptTrans = vm.CopyPixels(keptFile, 0, 0);
        kept = (byte[])vm.LastBitmap!.Data.Clone();

        byte[] g16File = Lump(0, 0, 3, 0, 0, 0, 1, 1, 16, 32, new byte[] { 0x11, 0x22 });
        vm.CopyPixels(g16File, 0, 0);
        g16 = (byte[])vm.LastBitmap!.Data.Clone();

        byte[] rgb16File = Lump(0, 0, 2, 0, 0, 0, 1, 1, 16, 32, new byte[] { 0x1F, 0x00 });
        vm.CopyPixels(rgb16File, 0, 0);
        rgb16 = (byte[])vm.LastBitmap!.Data.Clone();

        vm.Log.Add("---- edge");

        vm.Width = 2;
        vm.Height = 1;
        byte[] rle = Lump(0, 1, 9, 0, 1, 24, 2, 1, 8, 32, new byte[] { 0, 0, 255, 0x81, 0 });
        Assert.Equal(0, vm.CopyPixels(rle, 0, 0));
        Assert.Equal(new byte[] { 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF }, vm.LastBitmap!.Data);

        byte[] raw = Lump(0, 1, 9, 0, 1, 24, 2, 1, 8, 32, new byte[] { 0, 0, 255, 0x01, 0, 0 });
        Assert.Equal(0, vm.CopyPixels(raw, 0, 0));
        Assert.Equal(new byte[] { 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF }, vm.LastBitmap!.Data);

        vm.Width = 1;
        vm.Height = 1;
        byte[] comp = Lump(0, 0, 10, 0, 0, 0, 1, 1, 24, 32, new byte[] { 0x80, 9, 8, 7 });
        Assert.Equal(0, vm.CopyPixels(comp, 0, 0));
        Assert.Equal(new byte[] { 0x09, 0x08, 0x07, 0xFF }, vm.LastBitmap!.Data);

        vm.CopyPixels(null, 0, 0);
        Assert.Null(vm.LastBitmap);

        vm.CopyPixels(Array.Empty<byte>(), 0, 0);
        Assert.Null(vm.LastBitmap);

        byte[] id = Lump(100, 0, 1, 0, 0, 0, 1, 1, 8, 32, Array.Empty<byte>());
        vm.Fill = 0x5A;
        vm.CopyPixels(id, 0, 0);
        Assert.Equal(new byte[] { 0x5A, 0x5A, 0x5A, 0x5A }, vm.LastBitmap!.Data);

        byte[] far = Lump(0, 1, 1, 300, 1, 24, 1, 1, 8, 32, new byte[] { 1, 2, 3, 0 });
        vm.CopyPixels(far, 0, 0);
        Assert.Equal(new byte[] { 0x5A, 0x5A, 0x5A, 0x5A }, vm.LastBitmap!.Data);

        byte[] c15 = Lump(0, 1, 1, 0, 1, 15, 1, 1, 8, 32, new byte[] { 0xFF, 0x7F, 0 });
        vm.Fill = 0;
        vm.CopyPixels(c15, 0, 0);
        Assert.Equal(new byte[] { 0xF8, 0xF8, 0xF8, 0xFF }, vm.LastBitmap!.Data);

        byte[] c24 = Lump(0, 1, 1, 0, 1, 24, 1, 1, 8, 32, new byte[] { 1, 2, 3, 0 });
        vm.CopyPixels(c24, 0, 0);
        Assert.Equal(new byte[] { 0x01, 0x02, 0x03, 0xFF }, vm.LastBitmap!.Data);

        byte[] cdef = Lump(0, 1, 1, 0, 1, 0, 1, 1, 8, 32, new byte[] { 4 });
        vm.Fill = 0x5A;
        vm.CopyPixels(cdef, 0, 0);
        Assert.Equal(new byte[] { 0x5A, 0x5A, 0x5A, 0x5A }, vm.LastBitmap!.Data);

        byte[] skip = Lump(0, 0, 2, 0, 0, 0, 1, 1, 8, 32, new byte[] { 1, 2, 3 });
        vm.CopyPixels(skip, 0, 0);
        Assert.Equal(new byte[] { 0x5A, 0x5A, 0x5A, 0x5A }, vm.LastBitmap!.Data);

        byte[] neg = Lump(0, 1, 1, -1, 2, 24, 1, 1, 8, 32, new byte[] { 9, 9, 9, 1, 2, 3, 0 });
        vm.Fill = 0;
        vm.CopyPixels(neg, 0, 0);
        Assert.Equal(new byte[] { 0x01, 0x02, 0x03, 0xFF }, vm.LastBitmap!.Data);

        string dump = string.Join('\n', vm.Log) + "\n";
        return dump;
    }

    static byte[] Lump(byte idLen, byte hasCm, byte type, short first, short length, byte cmSize, short w, short h, byte bpp, byte desc, byte[] rest)
    {
        var buf = new List<byte>(18 + idLen + rest.Length);
        buf.Add(idLen);
        buf.Add(hasCm);
        buf.Add(type);
        buf.Add((byte)first);
        buf.Add((byte)(first >> 8));
        buf.Add((byte)length);
        buf.Add((byte)(length >> 8));
        buf.Add(cmSize);
        buf.Add(0);
        buf.Add(0);
        buf.Add(0);
        buf.Add(0);
        buf.Add((byte)w);
        buf.Add((byte)(w >> 8));
        buf.Add((byte)h);
        buf.Add((byte)(h >> 8));
        buf.Add(bpp);
        buf.Add(desc);
        for (int i = 0; i < idLen && i < 4; i++)
            buf.Add(0xAB);
        if (idLen > 4)
        {
            // The id field is the length only. A short file still records it.
        }
        buf.AddRange(rest);
        return buf.ToArray();
    }
}
