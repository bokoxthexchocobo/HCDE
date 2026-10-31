namespace HCDE.Playsim.Tests;

public class VmAnimLib1220SliceTests
{
    static void Put16(byte[] buffer, int at, ushort value)
    {
        buffer[at] = (byte)value;
        buffer[at + 1] = (byte)(value >> 8);
    }

    static void Put32(byte[] buffer, int at, uint value)
    {
        buffer[at] = (byte)value;
        buffer[at + 1] = (byte)(value >> 8);
        buffer[at + 2] = (byte)(value >> 16);
        buffer[at + 3] = (byte)(value >> 24);
    }

    static byte[] Lpf(int size, ushort pages, uint records)
    {
        var buffer = new byte[size];
        buffer[0] = (byte)'L';
        buffer[1] = (byte)'P';
        buffer[2] = (byte)'F';
        buffer[3] = (byte)' ';
        Put16(buffer, 6, pages);
        Put32(buffer, 8, records);
        return buffer;
    }

    [Fact]
    public void Load_RejectsAShortFileAndReadsThePalette()
    {
        var anim = new VmAnimLib1220();
        Assert.Equal(-1, anim.Load(new byte[] { (byte)'X', (byte)'X', (byte)'X', (byte)'X' }, 4));
        Assert.Equal(0, anim.CurrentFrame);

        var shortFile = Lpf(16, 0, 0);
        Assert.Equal(-1, anim.Load(shortFile, 16));
        Assert.Equal(0, anim.CurrentFrame);
        Assert.Equal(0, anim.CurPage);

        var tooMany = Lpf(1100, 100, 1);
        Assert.Equal(-2, anim.Load(tooMany, tooMany.Length));
        Assert.Equal(-1, anim.CurrentFrame);
        Assert.Equal((ushort)0xffff, anim.CurPage);
        Assert.Equal((ushort)100, anim.NLps);
        Assert.Equal(0, anim.Palette[0]);

        var empty = Lpf(2048, 0, 0);
        empty[256] = 1;
        empty[257] = 2;
        empty[258] = 3;
        Assert.Equal(-1, anim.Load(empty, empty.Length));
        Assert.Equal(3, anim.Palette[0]);
        Assert.Equal(2, anim.Palette[1]);
        Assert.Equal(1, anim.Palette[2]);
        Assert.Equal(0u, anim.NRecords);
    }

    [Fact]
    public void DrawFrame_DecodesFromThePreviousFrameAndKeepsSkippedPixels()
    {
        int fileSize = 0xb00 + 2 * VmAnimLib1220.ImageBufferSize;
        var buffer = Lpf(fileSize, 2, 3);
        Put16(buffer, 4, 256);
        Put16(buffer, 12, 2);
        Put16(buffer, 14, 1280);
        buffer[16] = (byte)'A';
        buffer[17] = (byte)'N';
        buffer[18] = (byte)'I';
        buffer[19] = (byte)'M';
        Put16(buffer, 20, 320);
        Put16(buffer, 22, 200);
        Put32(buffer, 64, 3);
        Put16(buffer, 68, 18);
        buffer[256] = 1;
        buffer[257] = 2;
        buffer[258] = 3;
        buffer[259] = 4;
        buffer[260] = 10;
        buffer[261] = 20;
        buffer[262] = 30;
        buffer[263] = 40;
        Put16(buffer, 1280, 0);
        Put16(buffer, 1282, 2);
        Put16(buffer, 1284, 100);
        Put16(buffer, 1286, 2);
        Put16(buffer, 1288, 1);
        Put16(buffer, 1290, 100);

        byte[] rec0 = { 0x03, 0x11, 0x22, 0x33, 0x80, 0x00, 0x00 };
        byte[] rec1 = { 0x00, 0x04, 0xAB, 0x81, 0x02, 0x44, 0x55, 0x80, 0x00, 0x00 };
        byte[] rec2 =
        {
            0x80, 0x02, 0x00,
            0x80, 0x03, 0xC0, 0x7E,
            0x80, 0x02, 0x80, 0x66, 0x77,
            0x80, 0x00, 0x00,
        };
        WritePage(buffer, 0, new[] { rec0, rec1 }, 0, 0);
        WritePage(buffer, 1, new[] { rec2 }, 0, 0);

        var anim = new VmAnimLib1220();
        Assert.Equal(0, anim.Load(buffer, buffer.Length));
        Assert.Equal(3u, anim.NRecords);
        Assert.Equal((ushort)320, anim.Width);
        Assert.Equal((ushort)200, anim.Height);
        Assert.Equal((ushort)18, anim.FramesPerSecond);
        Assert.Equal(new byte[] { 3, 2, 1, 30, 20, 10 }, anim.Palette.Take(6).ToArray());
        Assert.Equal((ushort)0, anim.BaseRecord[0]);
        Assert.Equal((ushort)2, anim.PageRecords[0]);
        Assert.Equal((ushort)2, anim.BaseRecord[1]);
        Assert.Equal((byte)'L', buffer[0]);

        var image = anim.DrawFrame(1);
        Assert.Same(anim.ImageBuffer, image);
        Assert.Equal(1, anim.CurrentFrame);
        Assert.Equal((ushort)0, anim.CurPage);
        Assert.Equal(new byte[] { 0x11, 0x22, 0x33, 0, 0, 0, 0, 0 }, image.Take(8).ToArray());

        anim.DrawFrame(1);
        Assert.Equal(new byte[] { 0xAB, 0xAB, 0xAB, 0xAB, 0x00, 0x44, 0x55, 0 }, image.Take(8).ToArray());

        anim.DrawFrame(3);
        Assert.Equal(3, anim.CurrentFrame);
        Assert.Equal((ushort)1, anim.CurPage);
        Assert.Equal(new byte[] { 0xAB, 0xAB, 0x7E, 0x7E, 0x7E, 0x66, 0x77, 0 }, image.Take(8).ToArray());

        anim.DrawFrame(0);
        Assert.Equal(0, anim.CurrentFrame);
        Assert.Equal((ushort)0, anim.CurPage);
        Assert.Equal(new byte[] { 0x11, 0x22, 0x33, 0x7E, 0x7E, 0x66, 0x77, 0 }, image.Take(8).ToArray());

        Put16(buffer, 1284, 1);
        var cut = new VmAnimLib1220();
        Assert.Equal(0, cut.Load(buffer, buffer.Length));
        for (int i = 0; i < 8; i++)
            cut.ImageBuffer[i] = 0x5A;
        cut.DrawFrame(0);
        Assert.Equal(new byte[] { 0x11, 0x22, 0x33, 0x5A, 0x5A, 0x5A, 0x5A, 0x5A }, cut.ImageBuffer.Take(8).ToArray());
        cut.DrawFrame(1);
        Assert.Equal(0x5A, cut.ImageBuffer[3]);

        var preamble = Lpf(0xb00 + VmAnimLib1220.ImageBufferSize, 1, 1);
        Put16(preamble, 1280, 0);
        Put16(preamble, 1282, 1);
        Put16(preamble, 1284, 50);
        int pp = 0xb00 + 8;
        Put16(preamble, pp, 8);
        preamble[pp + 3] = 1;
        Put16(preamble, pp + 4, 1);
        int data = pp + 6;
        preamble[data] = 0xEE;
        preamble[data + 1] = 0xEE;
        preamble[data + 2] = 0x02;
        preamble[data + 3] = 0x90;
        preamble[data + 4] = 0x91;
        preamble[data + 5] = 0x80;
        var odd = new VmAnimLib1220();
        Assert.Equal(0, odd.Load(preamble, preamble.Length));
        for (int i = 0; i < 8; i++)
            odd.ImageBuffer[i] = 0x5A;
        odd.DrawFrame(0);
        Assert.Equal(new byte[] { 0x90, 0x91, 0x5A, 0x5A, 0x5A, 0x5A, 0x5A, 0x5A }, odd.ImageBuffer.Take(8).ToArray());
    }

    static void WritePage(byte[] buffer, int page, byte[][] records, byte flag, ushort skip)
    {
        int pp = 0xb00 + page * VmAnimLib1220.ImageBufferSize + 8;
        int data = pp + records.Length * 2 + 4;
        int cursor = data;
        for (int i = 0; i < records.Length; i++)
        {
            Put16(buffer, pp + i * 2, (ushort)records[i].Length);
            System.Array.Copy(records[i], 0, buffer, cursor, records[i].Length);
            cursor += records[i].Length;
        }
        if (flag != 0)
        {
            buffer[pp + records.Length * 2 + 1] = flag;
            Put16(buffer, pp + records.Length * 2 + 2, skip);
        }
    }
}
