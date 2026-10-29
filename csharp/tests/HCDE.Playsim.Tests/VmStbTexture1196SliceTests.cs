namespace HCDE.Playsim.Tests;

public class VmStbTexture1196SliceTests
{
    [Fact]
    public void TryCreate_SeeksAndKeepsOnlyASuccessfulProbe()
    {
        VmStbTexture1196Slice.Reset();
        var file = new VmStbTexture1196Slice.Reader
        {
            Data = new byte[] { 1, 2, 3, 4, 5 },
            Position = 4,
        };
        var seen = new List<int>();
        VmStbTexture1196Slice.Probe = reader =>
        {
            seen.Add(reader.Position);
            return new VmStbTexture1196Slice.ProbeInfo { Result = 0, Width = 9, Height = 8, Components = 3 };
        };

        Assert.Null(VmStbTexture1196Slice.TryCreate(file, 12));
        Assert.Equal(0, file.Position);
        Assert.Equal(new[] { 0 }, seen);

        VmStbTexture1196Slice.Probe = reader =>
            new VmStbTexture1196Slice.ProbeInfo
            {
                Result = 2,
                Width = 9,
                Height = 8,
                Components = reader.Data.Length,
            };
        Assert.Null(VmStbTexture1196Slice.TryCreate(file, 12));

        VmStbTexture1196Slice.Probe = _ =>
            new VmStbTexture1196Slice.ProbeInfo { Result = 1, Width = 5, Height = 6, Components = 4 };
        var texture = VmStbTexture1196Slice.TryCreate(file, 12);
        Assert.NotNull(texture);
        Assert.Equal(12, texture.SourceLump);
        Assert.Equal(5, texture.Width);
        Assert.Equal(6, texture.Height);
        Assert.Equal(0, texture.LeftOffset);
        Assert.Equal(0, texture.TopOffset);

        file.Seek(0, VmStbTexture1196Slice.SeekSet);
        var buffer = new byte[10];
        Assert.Equal(2, file.Read(buffer, 2));
        Assert.Equal(new byte[] { 1, 2 }, buffer[..2]);
        file.Skip(1);
        Assert.Equal(3, file.Position);
        Assert.Equal(0, file.Eof());
        Assert.Equal(2, file.Read(buffer, 10));
        Assert.Equal(new byte[] { 4, 5 }, buffer[..2]);
        Assert.Equal(1, file.Eof());
    }

    [Fact]
    public void CreatePalettedPixels_TransposesClippedRgbaAndZerosLowAlpha()
    {
        VmStbTexture1196Slice.Reset();
        VmStbTexture1196Slice.Rgb256k[63, 0, 0] = 11;
        VmStbTexture1196Slice.Rgb256k[2, 5, 7] = 22;
        VmStbTexture1196Slice.Rgb256k[0, 0, 63] = 33;
        VmStbTexture1196Slice.Rgb256k[1, 0, 0] = 44;
        VmStbTexture1196Slice.Rgb256k[2, 2, 2] = 99;

        var image = new byte[3 * 4 * 4];
        Put(image, 0, 255, 0, 0, 255);
        Put(image, 1, 0, 255, 0, 127);
        Put(image, 2, 9, 9, 9, 255);
        Put(image, 3, 0, 0, 255, 0);
        Put(image, 4, 10, 20, 30, 128);
        Put(image, 5, 1, 2, 3, 255);
        Put(image, 6, 0, 0, 255, 200);
        Put(image, 7, 4, 0, 0, 255);
        Put(image, 8, 7, 7, 7, 255);
        Put(image, 9, 8, 8, 8, 255);
        Put(image, 10, 6, 6, 6, 255);
        Put(image, 11, 5, 5, 5, 255);

        var texture = new VmStbTexture1196Slice.Texture(4, 2, 3);
        VmStbTexture1196Slice.Lumps[4] = new byte[] { 1, 2, 3 };
        byte[]? opened = null;
        VmStbTexture1196Slice.Load = reader =>
        {
            opened = reader.Data;
            return new VmStbTexture1196Slice.RgbaImage { Width = 3, Height = 4, Pixels = image };
        };

        var bitmap = new VmStbTexture1196Slice.Bitmap();
        bitmap.Create(2, 3);
        Array.Fill(bitmap.Data, (byte)9);
        Assert.Equal(-1, texture.CopyPixels(bitmap, VmStbTexture1196Slice.Luminance, 4));
        Assert.Equal(new byte[] { 1, 2, 3 }, opened);
        Assert.Equal(
            new byte[]
            {
                0, 0, 255, 255, 0, 255, 0, 127,
                9, 9, 9, 9, 30, 20, 10, 128,
                255, 0, 0, 200, 0, 0, 4, 255,
            },
            bitmap.Data);

        Assert.Equal(new byte[] { 11, 0, 33, 0, 22, 44 }, texture.CreatePalettedPixels(0, 7));
        Assert.Equal(new byte[] { 76, 0, 36, 0, 18, 1 }, texture.CreatePalettedPixels(VmStbTexture1196Slice.Luminance));

        VmStbTexture1196Slice.Load = _ => null;
        Array.Fill(bitmap.Data, (byte)3);
        Assert.Equal(-1, texture.CopyPixels(bitmap, 0));
        Assert.All(bitmap.Data, value => Assert.Equal(3, value));
        Assert.Equal(new byte[] { 0, 0, 0, 0, 0, 0 }, texture.CreatePalettedPixels(0));
    }

    static void Put(byte[] dest, int index, byte r, byte g, byte b, byte a)
    {
        int offset = index * 4;
        dest[offset] = r;
        dest[offset + 1] = g;
        dest[offset + 2] = b;
        dest[offset + 3] = a;
    }
}
