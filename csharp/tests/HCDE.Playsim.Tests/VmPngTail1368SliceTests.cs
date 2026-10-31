namespace HCDE.Playsim.Tests;

public class VmPngTail1368SliceTests
{
    [Fact]
    public void IdatTailAndUnpack()
    {
        var slice = new VmPngTail1368Slice { ChunkLen = 4, Read = _ => throw new InvalidOperationException("unexpected") };
        slice.ContinueIdat();
        Assert.Equal(4u, slice.ChunkLen);
        slice.ChunkLen = 0;
        slice.LastIdat = true;
        slice.ContinueIdat();
        Assert.True(slice.LastIdat);
        slice.LastIdat = false;
        slice.Read = buf =>
        {
            Assert.Equal(12, buf.Length);
            return 4;
        };
        slice.ContinueIdat();
        Assert.True(slice.LastIdat);
        Assert.Equal(0u, slice.ChunkLen);
        slice.LastIdat = false;
        slice.Read = buf =>
        {
            buf[8] = (byte)'x';
            return 12;
        };
        slice.ContinueIdat();
        Assert.True(slice.LastIdat);
        Assert.Equal(0u, slice.ChunkLen);
        slice.LastIdat = false;
        slice.Read = buf =>
        {
            buf[4] = 0x00;
            buf[5] = 0x00;
            buf[6] = 0x01;
            buf[7] = 0x00;
            buf[8] = (byte)'I';
            buf[9] = (byte)'D';
            buf[10] = (byte)'A';
            buf[11] = (byte)'T';
            return 12;
        };
        slice.ContinueIdat();
        Assert.False(slice.LastIdat);
        Assert.Equal(256u, slice.ChunkLen);
        Assert.Equal(new[] { "Read", "Read", "Read" }, slice.Trace);
        var closed = new VmPngTail1368Slice();
        Assert.Throws<InvalidOperationException>(() => closed.ContinueIdat());

        slice.BitDepth = 4;
        slice.Pitch = 10;
        slice.Prev = 20;
        slice.Width = 3;
        slice.BytesPerRowIn = 2;
        slice.ColorType = 0;
        var offsets = new List<int>();
        var gray = new List<bool>();
        slice.Unpack = (width, rowBytes, depth, at, isGray) =>
        {
            Assert.Equal(3, width);
            Assert.Equal(2, rowBytes);
            Assert.Equal(slice.BitDepth, depth);
            offsets.Add(at);
            gray.Add(isGray);
        };
        slice.InflateEnd = () => { };
        slice.Trace.Clear();
        Assert.True(slice.FinishIdat());
        Assert.Equal(new[] { 0, 10, 20 }, offsets);
        Assert.Equal(new[] { true, true, true }, gray);
        Assert.Equal("inflateEnd", slice.Trace[0]);
        Assert.Equal(3, slice.Trace.Count(name => name == "UnpackPixels"));

        slice.BitDepth = 8;
        slice.Trace.Clear();
        offsets.Clear();
        Assert.True(slice.FinishIdat());
        Assert.Empty(offsets);
        Assert.Equal(new[] { "inflateEnd" }, slice.Trace);

        slice.BitDepth = 1;
        slice.Interlace = 1;
        slice.Prev = 10;
        slice.ColorType = 3;
        offsets.Clear();
        gray.Clear();
        Assert.True(slice.FinishIdat());
        Assert.Equal(new[] { 10 }, offsets);
        Assert.Equal(new[] { false }, gray);

        slice.Prev = 0;
        offsets.Clear();
        Assert.True(slice.FinishIdat());
        Assert.Empty(offsets);

        slice.Interlace = 0;
        slice.Pitch = 0;
        slice.Prev = 0;
        Assert.Throws<InvalidOperationException>(() => slice.FinishIdat());
        var noEnd = new VmPngTail1368Slice();
        Assert.Throws<InvalidOperationException>(() => noEnd.FinishIdat());
        noEnd.InflateEnd = () => { };
        noEnd.BitDepth = 4;
        noEnd.Pitch = 1;
        noEnd.Prev = 0;
        Assert.Throws<InvalidOperationException>(() => noEnd.FinishIdat());
    }

    [Fact]
    public void ChunkPaletteSumAndFilter()
    {
        var slice = new VmPngTail1368Slice();
        var storage = new byte[20];
        uint seenLen = 0;
        slice.Crc = (data, at, len) =>
        {
            Assert.Same(storage, data);
            Assert.Equal(4, at);
            seenLen = (uint)len;
            return 0x01020304;
        };
        slice.MakeChunk(storage, 8, VmPngTail1368Slice.IdatType, 4);
        Assert.Equal(new byte[] { 0, 0, 0, 4 }, storage[..4]);
        Assert.Equal(new byte[] { (byte)'I', (byte)'D', (byte)'A', (byte)'T' }, storage[4..8]);
        Assert.Equal(8u, seenLen);
        Assert.Equal(new byte[] { 0x01, 0x02, 0x03, 0x04 }, storage[12..16]);
        Assert.Equal(new[] { "CalcCRC32" }, slice.Trace);
        slice.Crc = (_, _, _) => throw new InvalidOperationException("CalcCRC32");
        var partial = new byte[16];
        Assert.Throws<InvalidOperationException>(() => slice.MakeChunk(partial, 8, 1, 1));
        Assert.Equal(0, partial[0]);
        Assert.NotEqual(0, partial[4] | partial[5] | partial[6] | partial[7]);

        var from = new byte[256 * 4];
        var to = new byte[256 * 3];
        for (int i = 0; i < 256; i++)
        {
            from[i * 4] = (byte)i;
            from[i * 4 + 1] = 2;
            from[i * 4 + 2] = 3;
            from[i * 4 + 3] = 9;
        }
        VmPngTail1368Slice.StuffPalette(from, to);
        Assert.Equal(768, to.Length);
        for (int i = 0; i < 256; i++)
        {
            Assert.Equal((byte)i, to[i * 3]);
            Assert.Equal((byte)2, to[i * 3 + 1]);
            Assert.Equal((byte)3, to[i * 3 + 2]);
        }
        Assert.Throws<InvalidOperationException>(() => VmPngTail1368Slice.StuffPalette(null!, to));
        Assert.Throws<IndexOutOfRangeException>(() => VmPngTail1368Slice.StuffPalette(new byte[4], to));

        Assert.Equal(0u, VmPngTail1368Slice.CalcSum(new byte[] { 9 }, 0));
        Assert.Equal(0xFFFFFFFFu, VmPngTail1368Slice.CalcSum(new byte[] { 255 }, 1));
        Assert.Equal(0u, VmPngTail1368Slice.CalcSum(new byte[] { 1, 255 }, 2));
        Assert.Throws<InvalidOperationException>(() => VmPngTail1368Slice.CalcSum(new byte[] { 1 }, -1));
        Assert.Throws<IndexOutOfRangeException>(() => VmPngTail1368Slice.CalcSum(new byte[] { 1 }, 2));

        var row = new byte[][] { new byte[] { 1, 2, 3 } };
        slice.UseFilterHeuristic = true;
        Assert.Equal(0, slice.SelectFilter(row, row[0], 1));
        Assert.Equal(new byte[] { 1, 2, 3 }, row[0]);
        Assert.Equal(0, slice.SelectFilter(null, null, 0));
    }
}
