namespace HCDE.Playsim.Tests;

public class VmPMapAimTraverseTests
{
    [Fact]
    public void ThreeDFloor_BlocksWhenPitchRangeClosed() =>
        Assert.False(VmPMapAimTraverse.Evaluate3dFloorBlock(Sample3d(top: 10, bottom: 5)));

    [Fact]
    public void ThreeDFloor_PassesWithout3dSectors() =>
        Assert.True(VmPMapAimTraverse.Evaluate3dFloorBlock(
            Sample3d(front3d: false, back3d: false)));

    [Fact]
    public void SectorPortal_SkipsHighCeilingPortal() =>
        Assert.False(VmPMapAimTraverse.EvaluateSectorPortalEntry(
            new VmPMapAimTraverse.SectorPortalContext(1, 32, 64, 1)));

    [Fact]
    public void LinePortal_BlocksWhenRestricted() =>
        Assert.False(VmPMapAimTraverse.EvaluateLinePortalEntry(
            new VmPMapAimTraverse.LinePortalContext(VmPMapAimTraverse.AlfPortalrestrict, false)));

    [Fact]
    public void MergeAimResults_PrefersCloserFrac() =>
        Assert.True(VmPMapAimTraverse.MergeAimResults(0.8, 0.2));

    [Fact]
    public void SectorPortalTrace_SetsAimUpOnCeiling() =>
        Assert.Equal(VmPMapAimTraverse.AimUp,
            VmPMapAimTraverse.SectorPortalAimDir(1, 1));

    [Fact]
    public void LinePortalTrace_ComputesTranslatedPosition() =>
        Assert.True(VmPMapAimTraverse.EvaluateLinePortalTrace(
            new VmPMapAimTraverse.LinePortalTraceContext(0, true, 0.5, 100, 10, 20, 1, 0)));

    [Fact]
    public void LeavingSectorUpperSlope_RaisesTopPitch() =>
        Assert.True(VmPMapAimTraverse.LeavingSectorUpperSlope(true, true, true, 0.5, 0.2));

    [Fact]
    public void SkipPortalFrac_BiasesPastTransition() =>
        Assert.Equal(0.51, VmPMapAimTraverse.SkipPortalFracBias(0.5, 100));

    private static VmPMapAimTraverse.Aim3dFloorContext Sample3d(
        double top = 5,
        double bottom = 10,
        bool front3d = true,
        bool back3d = false) =>
        new(true, front3d, back3d, false, true, 0, 0, top, bottom, false, 0, false);
}
