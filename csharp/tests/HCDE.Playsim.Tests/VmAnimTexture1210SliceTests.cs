namespace HCDE.Playsim.Tests;

public class VmAnimTexture1210SliceTests
{
    public VmAnimTexture1210SliceTests()
    {
        VmAnimTextures1210Slice.FindGameTexture = null;
    }

    static byte[] Pix(params byte[] bytes) => bytes;

    [Fact]
    public void SetFrame_CopiesYuvRgbPaletteAndVpxPlanes()
    {
        var tex = new VmAnimTexture1210Slice();
        tex.SetFrameSize(VmAnimTexture1210Slice.Yuv, 1, 7);
        Assert.Equal(28, tex.Pixels.Length);
        Assert.All(tex.Pixels, b => Assert.Equal(0, b));
        tex.SetFrame(null, Pix(
            0, 0, 0, 99,
            16, 128, 128, 99,
            235, 128, 128, 99,
            81, 90, 240, 99,
            210, 16, 146, 99,
            255, 255, 255, 99,
            126, 1, 200, 99));
        Assert.Equal(Pix(
            0, 135, 0, 255,
            1, 0, 0, 255,
            255, 254, 255, 255,
            0, 0, 255, 255,
            1, 254, 255, 255,
            255, 124, 255, 255,
            0, 118, 243, 255), tex.Pixels);

        tex.SetFrame(null, null);
        Assert.Equal(0, tex.Pixels[0]);
        Assert.Equal(135, tex.Pixels[1]);

        tex.SetFrameSize(VmAnimTexture1210Slice.Rgb, 2, 2);
        Assert.Equal(16, tex.Pixels.Length);
        Assert.All(tex.Pixels, b => Assert.Equal(0, b));
        tex.SetFrame(null, Pix(4, 5, 6, 7, 8, 9));
        Assert.Equal(Pix(
            6, 5, 4, 255,
            6, 5, 4, 255,
            6, 5, 4, 255,
            6, 5, 4, 255), tex.Pixels);

        tex.SetFrameSize(VmAnimTexture1210Slice.Paletted, 1, 2);
        Assert.Throws<InvalidOperationException>(() => tex.SetFrame(null, Pix(0, 1)));
        Assert.All(tex.Pixels, b => Assert.Equal(0, b));
        tex.SetFrame(Pix(10, 20, 30, 1, 2, 3), Pix(0, 1));
        Assert.Equal(Pix(30, 20, 10, 255, 3, 2, 1, 255), tex.Pixels);
        var remap = Pix(9, 8, 7);
        var trans = new[] { 4 };
        var bitmap = tex.GetBgraBitmap(remap, trans);
        Assert.Same(tex.Pixels, bitmap.Data);
        Assert.Equal(4, bitmap.Pitch);
        Assert.Equal(1, bitmap.Width);
        Assert.Equal(2, bitmap.Height);
        Assert.Equal(Pix(9, 8, 7), remap);
        Assert.Equal(4, trans[0]);
        bitmap.Data[0] = 77;
        Assert.Equal(77, tex.Pixels[0]);

        tex.SetFrameSize(VmAnimTexture1210Slice.Vpx, 2, 2);
        var i420 = new VmVpxImage1210
        {
            Fmt = VmVpxImage1210.I420,
            Y = Pix(16, 235, 7, 16, 235),
            U = Pix(128, 90, 255, 255),
            V = Pix(128, 240, 255, 255),
            YStride = 3,
            UStride = 2,
            VStride = 2,
        };
        tex.SetFrame(null, i420);
        Assert.Equal(Pix(
            1, 0, 0, 255,
            255, 254, 255, 255,
            1, 0, 0, 255,
            255, 254, 255, 255), tex.Pixels);
        var kept = (byte[])tex.Pixels.Clone();
        i420.Fmt = 1;
        tex.SetFrame(null, i420);
        tex.SetFrame(null, null);
        Assert.Equal(kept, tex.Pixels);

        var i444 = new VmVpxImage1210
        {
            Fmt = VmVpxImage1210.I444,
            Y = Pix(16, 235, 99, 81, 210),
            U = Pix(128, 128, 1, 90, 16),
            V = Pix(128, 128, 2, 240, 146),
            YStride = 3,
            UStride = 3,
            VStride = 3,
        };
        tex.SetFrame(null, i444);
        Assert.Equal(Pix(
            1, 0, 0, 255,
            255, 254, 255, 255,
            0, 0, 255, 255,
            1, 254, 255, 255), tex.Pixels);

        tex.SetFrameSize(99, 1, 1);
        tex.SetFrame(null, Pix(1, 2, 3, 4));
        Assert.Equal(Pix(0, 0, 0, 0), tex.Pixels);

        tex.SetFrameSize(VmAnimTexture1210Slice.Vpx, 2, 2);
        tex.SetFrame(null, new VmVpxImage1210
        {
            Fmt = VmVpxImage1210.I422,
            Y = Pix(0, 0, 126, 126),
            U = Pix(0, 255, 1, 255),
            V = Pix(0, 255, 200, 255),
            YStride = 2,
            UStride = 2,
            VStride = 2,
        });
        Assert.Equal(Pix(
            0, 135, 0, 255,
            0, 135, 0, 255,
            0, 118, 243, 255,
            0, 118, 243, 255), tex.Pixels);

        tex.SetFrame(null, new VmVpxImage1210
        {
            Fmt = VmVpxImage1210.I440,
            Y = Pix(0, 16, 0, 16),
            U = Pix(0, 128, 255, 255),
            V = Pix(0, 128, 255, 255),
            YStride = 2,
            UStride = 2,
            VStride = 2,
        });
        Assert.Equal(Pix(
            0, 135, 0, 255,
            1, 0, 0, 255,
            0, 135, 0, 255,
            1, 0, 0, 255), tex.Pixels);
    }

    [Fact]
    public void SetFrame_SwapsTheTwoTexturesAndCleansHardware()
    {
        var seen = new List<(string Name, int Use)>();
        VmAnimGameTexture1210? left = null;
        VmAnimGameTexture1210? right = null;
        VmAnimTextures1210Slice.FindGameTexture = (name, use) =>
        {
            seen.Add((name, use));
            var game = new VmAnimGameTexture1210(new VmAnimTexture1210Slice(), seen.Count);
            if (seen.Count == 1)
                left = game;
            else
                right = game;
            return game;
        };

        var pair = new VmAnimTextures1210Slice();
        Assert.Equal(
            new[] { ("AnimTextureFrame1", 10), ("AnimTextureFrame2", 10) },
            seen);
        Assert.Equal(1, pair.Active);
        Assert.Same(right, pair.GetFrame());
        Assert.NotNull(left);
        Assert.NotNull(right);
        left!.Materials.Add(true);
        left.Materials.Add(false);

        pair.SetSize(VmAnimTexture1210Slice.Yuv, 1, 1);
        Assert.Equal(1, left.TexelWidth);
        Assert.Equal(1f, left.DisplayWidth);
        Assert.Equal(1f, left.ScaleX);
        Assert.Equal(1f, right!.ScaleY);
        Assert.Equal(1, left.CleanCalls);
        Assert.Equal(1, right.CleanCalls);
        Assert.True(left.LastCleanFull);
        Assert.Equal(1, left.BaseHardwareCleans);
        Assert.Equal(1, left.DescriptorDeletes);
        Assert.Equal(0, right.DescriptorDeletes);
        Assert.Equal(Pix(0, 0, 0, 0), left.Base.Pixels);
        Assert.Equal(Pix(0, 0, 0, 0), right.Base.Pixels);

        pair.SetFrame(null, Pix(0, 0, 0, 99));
        Assert.Equal(0, pair.Active);
        Assert.Same(left, pair.GetFrame());
        Assert.Equal(Pix(0, 135, 0, 255), left.Base.Pixels);
        Assert.Equal(Pix(0, 0, 0, 0), right.Base.Pixels);
        Assert.Equal(2, left.CleanCalls);
        Assert.Equal(1, right.CleanCalls);

        pair.SetFrame(null, Pix(16, 128, 128, 99));
        Assert.Equal(1, pair.Active);
        Assert.Same(right, pair.GetFrame());
        Assert.Equal(Pix(0, 135, 0, 255), left.Base.Pixels);
        Assert.Equal(Pix(1, 0, 0, 255), right.Base.Pixels);
        Assert.Equal(2, right.CleanCalls);

        pair.SetSize(VmAnimTexture1210Slice.Paletted, 1, 1);
        Assert.Equal(1, pair.Active);
        Assert.Equal(Pix(0, 0, 0, 0), left.Base.Pixels);
        Assert.Equal(3, left.CleanCalls);
        Assert.Equal(3, right.CleanCalls);
        Assert.Throws<InvalidOperationException>(() => pair.SetFrame(null, Pix(0)));
        Assert.Equal(0, pair.Active);
        Assert.Same(left, pair.GetFrame());
        Assert.Equal(3, left.CleanCalls);
        Assert.Equal(Pix(0, 0, 0, 0), right.Base.Pixels);

        pair.Dispose();
        Assert.Null(pair.GetFrame());
        Assert.Equal(0, pair.Active);
        Assert.True(left.LastCleanFull);
        Assert.True(right.LastCleanFull);
        Assert.Throws<NullReferenceException>(() => pair.SetSize(VmAnimTexture1210Slice.Yuv, 1, 1));

        VmAnimTextures1210Slice.FindGameTexture = (_, _) => null;
        var missing = new VmAnimTextures1210Slice();
        Assert.Equal(1, missing.Active);
        Assert.Null(missing.GetFrame());
        Assert.Throws<NullReferenceException>(() => missing.SetFrame(null, Pix(0, 0, 0, 0)));
        Assert.Equal(0, missing.Active);
        missing.Clean();

        VmAnimTextures1210Slice.FindGameTexture = null;
        Assert.Throws<NullReferenceException>(() => new VmAnimTextures1210Slice());
    }
}
