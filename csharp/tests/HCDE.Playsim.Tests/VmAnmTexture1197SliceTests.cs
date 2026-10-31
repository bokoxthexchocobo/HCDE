namespace HCDE.Playsim.Tests;

public class VmAnmTexture1197SliceTests
{
    [Fact]
    public void TryCreate_RequiresLpfAndAtLeastOneFrame()
    {
        VmAnmTexture1197Slice.Reset();
        var loads = new List<(int Length, int Padded)>();
        var draws = new List<int>();
        VmAnmTexture1197Slice.Load = (buffer, length) =>
        {
            loads.Add((length, buffer.Length));
            return new VmAnmTexture1197Slice.Anim
            {
                Result = 0,
                Frames = length == 8 ? 0 : 1,
                Draw = frame =>
                {
                    draws.Add(frame);
                    return Array.Empty<byte>();
                },
            };
        };

        Assert.Null(VmAnmTexture1197Slice.TryCreate(ReaderOf(new byte[] { (byte)'L', (byte)'P' }), 3));
        Assert.Empty(loads);

        var wrong = ReaderOf("LPFX"u8.ToArray());
        wrong.Position = 2;
        Assert.Null(VmAnmTexture1197Slice.TryCreate(wrong, 3));
        Assert.Empty(loads);
        Assert.Equal(4, wrong.Position);

        var shortRead = ReaderOf("LPF extra"u8.ToArray());
        shortRead.ReportedLength = 40;
        Assert.Null(VmAnmTexture1197Slice.TryCreate(shortRead, 3));
        Assert.Empty(loads);

        var none = ReaderOf("LPF none"u8.ToArray());
        Assert.Null(VmAnmTexture1197Slice.TryCreate(none, 3));
        Assert.Equal((8, 9), loads[^1]);
        Assert.Equal(1, loads[^1].Padded - loads[^1].Length);

        VmAnmTexture1197Slice.Load = (buffer, length) =>
        {
            loads.Add((length, buffer.Length));
            return new VmAnmTexture1197Slice.Anim { Result = -1, Frames = 4 };
        };
        Assert.Null(VmAnmTexture1197Slice.TryCreate(ReaderOf("LPF fail!"u8.ToArray()), 9));

        VmAnmTexture1197Slice.Load = (buffer, length) =>
        {
            loads.Add((length, buffer.Length));
            Assert.Equal(0, buffer[^1]);
            return new VmAnmTexture1197Slice.Anim
            {
                Result = 0,
                Frames = 2,
                Draw = frame =>
                {
                    draws.Add(frame);
                    return Array.Empty<byte>();
                },
            };
        };
        var made = VmAnmTexture1197Slice.TryCreate(ReaderOf("LPF good"u8.ToArray()), 9);
        Assert.NotNull(made);
        Assert.Equal(9, made.SourceLump);
        Assert.Equal(320, made.Width);
        Assert.Equal(200, made.Height);
        Assert.Equal(0, made.LeftOffset);
        Assert.Equal(0, made.TopOffset);
        Assert.Equal((8, 9), loads[^1]);
        Assert.Empty(draws);
    }

    [Fact]
    public void ReadFrame_CopiesFrameOneAndPaintsColumnMajorPalette()
    {
        VmAnmTexture1197Slice.Reset();
        var lump = "LPF data"u8.ToArray();
        VmAnmTexture1197Slice.Lumps[4] = lump;
        var palette = new byte[768];
        palette[0] = 1;
        palette[1] = 2;
        palette[2] = 3;
        palette[3] = 10;
        palette[4] = 20;
        palette[5] = 30;
        palette[6] = 40;
        palette[7] = 50;
        palette[8] = 60;
        palette[9] = 70;
        palette[10] = 80;
        palette[11] = 90;
        var draws = new List<int>();
        var lengths = new List<int>();
        VmAnmTexture1197Slice.Load = (buffer, length) =>
        {
            lengths.Add(length);
            Assert.Equal(lump, buffer);
            return new VmAnmTexture1197Slice.Anim
            {
                Result = 0,
                Frames = 3,
                Palette = palette,
                Draw = frame =>
                {
                    draws.Add(frame);
                    return new byte[] { 1, 2, 3, 0 };
                },
            };
        };
        VmAnmTexture1197Slice.Pick = (r, g, b) => (byte)(r + g + b);

        var texture = new VmAnmTexture1197Slice.Texture(4, 2, 2);
        var bitmap = new VmAnmTexture1197Slice.Bitmap { Data = new byte[20] };
        Array.Fill(bitmap.Data, (byte)7);
        Assert.Equal(-1, texture.CopyPixels(bitmap, 1, 9));
        Assert.Equal(new[] { lump.Length }, lengths);
        Assert.Equal(new[] { 1 }, draws);
        Assert.Equal(
            new byte[]
            {
                30, 20, 10, 255,
                60, 50, 40, 255,
                90, 80, 70, 255,
                3, 2, 1, 255,
                7, 7, 7, 7,
            },
            bitmap.Data);

        Assert.Equal(new byte[] { 60, 240, 150, 6 }, texture.CreatePalettedPixels(1, 4));
        Assert.Equal(2, draws.Count);
        Assert.Equal(1, draws[^1]);

        VmAnmTexture1197Slice.Load = (_, _) => new VmAnmTexture1197Slice.Anim { Result = 0, Frames = 0, Palette = palette };
        Array.Fill(bitmap.Data, (byte)7);
        Assert.Equal(-1, texture.CopyPixels(bitmap, 0));
        Assert.Equal(
            new byte[]
            {
                0, 0, 0, 255, 0, 0, 0, 255, 0, 0, 0, 255, 0, 0, 0, 255, 7, 7, 7, 7,
            },
            bitmap.Data);
        VmAnmTexture1197Slice.Pick = (_, _, _) => 5;
        Assert.Equal(new byte[] { 5, 5, 5, 5 }, texture.CreatePalettedPixels(0));

        var missing = new VmAnmTexture1197Slice.Texture(99, 1, 1);
        var error = Assert.Throws<VmAnmTexture1197Slice.MissingLump>(() => missing.CopyPixels(bitmap, 0));
        Assert.Equal("ReadFile: 99 >= NumEntries", error.Detail);
    }

    static VmAnmTexture1197Slice.Reader ReaderOf(byte[] data) => new() { Data = data };
}
