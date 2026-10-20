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

    private static VmPMapAimTraverse.Aim3dFloorContext Sample3d(
        double top = 5,
        double bottom = 10,
        bool front3d = true,
        bool back3d = false) =>
        new(true, front3d, back3d, false, true, 0, 0, top, bottom, false, 0, false);
}
