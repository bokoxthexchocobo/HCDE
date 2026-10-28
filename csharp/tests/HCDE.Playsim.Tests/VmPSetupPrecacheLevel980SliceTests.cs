namespace HCDE.Playsim.Tests;

public class VmPSetupPrecacheLevel980SliceTests
{
    [Fact]
    public void PrecacheLevel_SkipAndRendererPath()
    {
        Assert.True(VmPSetupPrecacheLevel980Slice.ShouldSkipPrecacheLevel(true, false));
        Assert.True(VmPSetupPrecacheLevel980Slice.UseSoftwareRendererPrecachePath(hardwareRenderer: false));
        Assert.False(VmPSetupPrecacheLevel980Slice.UseSoftwareRendererPrecachePath(true));
    }

    [Fact]
    public void SideSkyTransferHitflag()
    {
        var flags = VmPSetupPrecacheLevel980Slice.MergeSidePrecacheHitflags(
            VmPSetupPrecacheLevel980Slice.HitWall,
            skyTransferActive: true);
        Assert.Equal(VmPSetupPrecacheLevel980Slice.HitWall | VmPSetupPrecacheLevel980Slice.HitSky, flags);
        Assert.True(VmPSetupPrecacheLevel980Slice.SideQualifiesForSkyTransferHitflag(
            sideIsFirstOnLine: true,
            lineSpecial: 271,
            staticInitSpecial: 271,
            transferSkyArg: VmPSetupPrecacheLevel980Slice.InitTransferSky));
    }

    [Fact]
    public void PortalDefaultsAndCoverage()
    {
        Assert.Equal(2, VmPSetupPrecacheLevel980Slice.MinimumSectorPortalSlotsForDefaults(0));
        Assert.True(VmPSetupPrecacheLevel980Slice.DefaultSkyPortalUsesSkyFlatOnly(0, portalCount: 2));
        Assert.True(VmPSetupPrecacheLevel980Slice.ShouldReleaseSubsectorPortalCoverage(new object[1]));
    }

    [Fact]
    public void ClearLevelData_ResetsStatistics()
    {
        var stats = VmPSetupPrecacheLevel980Slice.ResetLevelStatistics();
        Assert.Equal((0, 0, 0, 0, 0, 0), stats);
        var sky = VmPSetupPrecacheLevel980Slice.ResetSkyScrollPositions();
        Assert.Equal((0.0, 0.0, 0.0, 0.0, 0.0), sky);
        Assert.True(VmPSetupPrecacheLevel980Slice.ClearLevelDataScrubsNativePointers(
            probeNotNull: true,
            nativeFieldCount: 8));
    }

    [Fact]
    public void PrecacheClassHitlist_MergesOnce()
    {
        var map = new Dictionary<object, bool>();
        var cls = new object();
        Assert.True(VmPSetupPrecacheLevel980Slice.TryMergePrecacheClassEntry(map, cls, out var added));
        Assert.True(added);
        Assert.True(VmPSetupPrecacheLevel980Slice.TryMergePrecacheClassEntry(map, cls, out added));
        Assert.False(added);
        Assert.True(VmPSetupPrecacheLevel980Slice.ShouldPrecacheLevelSkyTexture(true, textureIndex: 1));
    }
}
