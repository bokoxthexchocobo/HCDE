namespace HCDE.Playsim.Tests;

public class VmPLnspecCameraProperty932SliceTests
{
    [Fact]
    public void AssignActivatorCamera_ClearsRevertWhenNoCamera()
    {
        var (_, cheats) = VmPLnspecCameraProperty932Slice.AssignActivatorCamera(null, new object(), CfWithRevert(), 1);
        Assert.Equal(0, cheats);
    }

    [Fact]
    public void ShouldRejectPropertyCall_WhenNoActivatorPlayer()
    {
        Assert.True(VmPLnspecCameraProperty932Slice.ShouldRejectPropertyCall(false, 0));
    }

    [Fact]
    public void PowerBranch_RejectsInvalidPowerSlot()
    {
        Assert.False(VmPLnspecCameraProperty932Slice.PowerEntryValid(8));
    }

    [Fact]
    public void AllmapGrant_UpdatesLevelFlags()
    {
        var which = VmPLnspecCameraProperty932Slice.PropInvulnerability + 4;
        var flags = VmPLnspecCameraProperty932Slice.AllmapFlags2ForGrant(0, which, 1, true);
        Assert.Equal(VmPLnspecCameraProperty932Slice.Level2Allmap, flags);
    }

    [Fact]
    public void InvulnerabilityBlend_AppliesOnGive()
    {
        Assert.True(VmPLnspecCameraProperty932Slice.InvulnerabilityBlendApplies(
            VmPLnspecCameraProperty932Slice.PropInvulnerability, 1));
    }

    [Fact]
    public void ShouldClearViewer_WhenReferenceChanges()
    {
        var a = new object();
        var b = new object();
        Assert.True(VmPLnspecCameraProperty932Slice.ShouldClearViewer(a, b));
    }

    private static int CfWithRevert() => VmPLnspecCameraProperty932Slice.CfRevertPlease;
}
