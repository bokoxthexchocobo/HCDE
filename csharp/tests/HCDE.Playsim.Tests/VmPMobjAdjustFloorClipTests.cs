namespace HCDE.Playsim.Tests;

public class VmPMobjAdjustFloorClipTests
{
    [Fact]
    public void Skip_SpecialFloorclipFlag() =>
        Assert.True(VmPMobjAdjustFloorClip.SkipWhenSpecialFloorclip(VmPMobjAdjustFloorClip.Mf3Specialfloorclip));

    [Fact]
    public void TerrainClip_PicksShallowest() =>
        Assert.Equal(4, VmPMobjAdjustFloorClip.PickShallowestClip(8, 4));

    [Fact]
    public void Resolve_NoTouchingReturnsZero() =>
        Assert.Equal(0, VmPMobjAdjustFloorClip.ResolveFloorClip(10, false));

    [Fact]
    public void ThreeDFloor_RequiresSolidExists() =>
        Assert.True(VmPMobjAdjustFloorClip.Valid3dFloor(
            VmPMobjAdjustFloorClip.FfSolid | VmPMobjAdjustFloorClip.FfExists));

    [Fact]
    public void PlayerView_AdjustsWhenClipChanges() =>
        Assert.True(VmPMobjAdjustFloorClip.PlayerViewAdjust(true, true, 8, 4));
}
