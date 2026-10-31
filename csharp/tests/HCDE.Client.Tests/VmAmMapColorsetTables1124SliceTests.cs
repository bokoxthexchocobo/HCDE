namespace HCDE.Client.Tests;

public class VmAmMapColorsetTables1124SliceTests
{
    [Fact]
    public void InitFromColorTriplets_MarksUnusedAndClearsFillAlpha()
    {
        var bytes = new byte[VmAmMapColorsetTables1124Slice.ColorCount * 3];
        bytes[0] = 1;
        bytes[3] = 0xff;
        bytes[4] = 0x10;
        bytes[5] = 0x20;
        var packed = VmAmMapColorsetTables1124Slice.InitFromColorTriplets(
            bytes, true, out var alpha, out var locks, out var forceBackground);
        Assert.Equal(0u, packed[0]);
        Assert.Equal(VmAmMapColorsetTables1124Slice.PackOpaqueRgb(0xff, 0x10, 0x20), packed[1]);
        Assert.Equal(0f, alpha);
        Assert.True(locks);
        Assert.False(forceBackground);
    }

    [Fact]
    public void SetWhite_BlacksBackgroundAndWhitesTheRest()
    {
        var packed = VmAmMapColorsetTables1124Slice.SetWhite();
        Assert.Equal(VmAmMapColorsetTables1124Slice.PackOpaqueRgb(0, 0, 0), packed[0]);
        Assert.True(VmAmMapColorsetTables1124Slice.IsValidIndex(packed, 0));
        Assert.False(VmAmMapColorsetTables1124Slice.IsValidIndex(new uint[] { 0 }, 0));
        Assert.Equal(VmAmMapColorsetTables1124Slice.PackOpaqueRgb(255, 255, 255), packed[25]);
    }

    [Fact]
    public void LineStyleColorIndex_MapsPortalAndDefault()
    {
        Assert.Equal(-1, VmAmMapColorsetTables1124Slice.LineStyleColorIndex(0));
        Assert.Equal(VmAmMapColorsetTables1124Slice.PortalColor, VmAmMapColorsetTables1124Slice.LineStyleColorIndex(13));
        Assert.Equal(14, VmAmMapColorsetTables1124Slice.LineStyleColors().Count);
    }

    [Fact]
    public void RestoreColorDefaults_ResetsBothTablesAndBothAlphas()
    {
        var names = new List<string>();
        var alphas = 0;
        var count = VmAmMapColorsetTables1124Slice.RestoreColorDefaults(
            names.Add,
            () => alphas++,
            () => alphas++);
        Assert.Equal(50, count);
        Assert.Equal(2, alphas);
        Assert.Equal("am_backcolor", names[0]);
        Assert.Equal("am_portalcolor", names[24]);
        Assert.Equal("am_ovportalcolor", names[^1]);
        Assert.Equal((byte)0, VmAmMapColorsetTables1124Slice.DoomBackgroundTriplet().R);
    }
}
