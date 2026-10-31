namespace HCDE.Playsim.Tests;

public class VmMd5Check1214SliceTests
{
    [Fact]
    public void MakeMd5_PrintsLowerHexAndTheAlphaListRejectsOtherBytes()
    {
        Assert.Equal("d41d8cd98f00b204e9800998ecf8427e", VmMd5Check1214Slice.MakeMd5(Array.Empty<byte>(), 0));
        Assert.Equal("900150983cd24fb0d6963f7d28e17f72", VmMd5Check1214Slice.MakeMd5(new byte[] { (byte)'a', (byte)'b', (byte)'c' }, 3));
        var zeros = new byte[18];
        Assert.Equal("ff035bff2dcf972ee7dfd023455997ef", VmMd5Check1214Slice.MakeMd5(zeros, 18));
        Assert.Equal(127, VmMd5Check1214Slice.AlphaPatches.Length);
        Assert.Equal((18, "fb1472adb34ce1de7a40156d484dd3fc"), VmMd5Check1214Slice.AlphaPatches[0]);
        Assert.Equal((10164, "3c8908619cd5b357de701a0c6b04f408"), VmMd5Check1214Slice.AlphaPatches[^1]);
        Assert.Equal(2, VmMd5Check1214Slice.PalImgzs.Length);
        Assert.Equal((1728, "9ffdd749b414a1d34c74650828ad6504"), VmMd5Check1214Slice.PalImgzs[0]);
        Assert.Equal((64024, "bee2a3ddec08bf975353b3dc139bb956"), VmMd5Check1214Slice.PalImgzs[1]);

        Assert.False(VmMd5Check1214Slice.CheckPatchForAlpha(null, 10165));
        Assert.False(VmMd5Check1214Slice.CheckPatchForAlpha(null, 1));
        Assert.False(VmMd5Check1214Slice.CheckPatchForAlpha(zeros, 18));
        Assert.False(VmMd5Check1214Slice.CheckPatchForAlpha(new byte[105], 105));
        Assert.False(VmMd5Check1214Slice.CheckPatchForAlpha(new byte[10164], 10164));
        Assert.Throws<NullReferenceException>(() => VmMd5Check1214Slice.CheckPatchForAlpha(null, 18));

        string hit = VmMd5Check1214Slice.MakeMd5(new byte[] { 1, 2, 3, 4 }, 4);
        Assert.True(VmMd5Check1214Slice.MatchListed(new byte[] { 1, 2, 3, 4 }, 4, new[] { (4, hit) }));
        Assert.True(VmMd5Check1214Slice.MatchListed(
            new byte[] { 1, 2, 3, 4 },
            4,
            new[] { (4, "00000000000000000000000000000000"), (9, hit), (4, hit) }));
        Assert.False(VmMd5Check1214Slice.MatchListed(new byte[] { 1, 2, 3, 4 }, 4, new[] { (4, "00000000000000000000000000000000") }));
    }

    [Fact]
    public void CheckImgzPalette_HashesFromTheStartOnlyWhenTheLengthIsListed()
    {
        var idle = new VmImgzReader1214 { Bytes = new byte[] { 1, 2, 3 } };
        Assert.False(VmMd5Check1214Slice.CheckImgzPalette(idle));
        Assert.Equal(0, idle.Seeks);
        Assert.Equal(0, idle.ReadCalls);

        var zeros = new VmImgzReader1214 { Bytes = new byte[1728] };
        Assert.False(VmMd5Check1214Slice.CheckImgzPalette(zeros));
        Assert.Equal(1, zeros.Seeks);
        Assert.Equal(1, zeros.ReadCalls);
        Assert.Equal(1728, zeros.Position);

        var missed = new VmImgzReader1214 { Bytes = new byte[64024] };
        Assert.False(VmMd5Check1214Slice.CheckImgzPalette(missed));
        Assert.Equal(1, missed.Seeks);

        byte[] picture = { 9, 8, 7, 6 };
        string digest = VmMd5Check1214Slice.MakeMd5(picture, 4);
        var file = new VmImgzReader1214 { Bytes = picture, Position = 3 };
        Assert.True(VmMd5Check1214Slice.CheckImgzPalette(file, new[] { (9, digest), (4, "00000000000000000000000000000000"), (4, digest) }));
        Assert.Equal(1, file.Seeks);
        Assert.Equal(1, file.ReadCalls);
        Assert.Equal(4, file.Position);

        var unread = new VmImgzReader1214 { Bytes = null };
        Assert.False(VmMd5Check1214Slice.CheckImgzPalette(unread));
        Assert.Throws<NullReferenceException>(() => VmMd5Check1214Slice.CheckImgzPalette(new VmImgzReader1214 { Bytes = null }, new[] { (0, digest) }));
    }
}
