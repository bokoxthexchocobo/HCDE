namespace HCDE.Playsim.Tests;

public class VmActorPrecalcAnimationTests
{
    [Fact]
    public void ShouldFindAnimationFrame_RequiresLayerAndNonNegativeTic()
    {
        Assert.False(VmActorPrecalcAnimation.ShouldFindAnimationFrame(false, 0));
        Assert.False(VmActorPrecalcAnimation.ShouldFindAnimationFrame(true, -1));
        Assert.True(VmActorPrecalcAnimation.ShouldFindAnimationFrame(true, 0));
    }

    [Fact]
    public void FindAnimationFrameUiTic_AddsFracWhenUnpaused()
    {
        Assert.Equal(10.5, VmActorPrecalcAnimation.FindAnimationFrameUiTic(10, false, false, 0.5));
        Assert.Equal(10, VmActorPrecalcAnimation.FindAnimationFrameUiTic(10, true, false, 0.5));
    }

    [Fact]
    public void CanBlendPrecalcFrameData_RequiresEqualSizes()
    {
        Assert.True(VmActorPrecalcAnimation.CanBlendPrecalcFrameData(3, 3));
        Assert.False(VmActorPrecalcAnimation.CanBlendPrecalcFrameData(2, 3));
    }

    [Fact]
    public void OffsetBoneTranslation_AddsComponents()
    {
        var (x, y, z) = VmActorPrecalcAnimation.OffsetBoneTranslation(1, 2, 3, 4, 5, 6);
        Assert.Equal((5, 7, 9), (x, y, z));
    }

    [Fact]
    public void MultiplyQuaternion_IdentityTimesRotation()
    {
        var (w, x, y, z) = VmActorPrecalcAnimation.MultiplyQuaternion(1, 0, 0, 0, 0, 1, 0, 0);
        Assert.Equal(0, w);
        Assert.Equal(1, x);
    }

    [Fact]
    public void ScanLegacyRenderStyleIndex_FindsMatch()
    {
        ReadOnlySpan<int> table = [10, 20, 30];
        Assert.Equal(1, VmActorPrecalcAnimation.ScanLegacyRenderStyleIndex(table, 20));
        Assert.Equal(-1, VmActorPrecalcAnimation.ScanLegacyRenderStyleIndex(table, 99));
    }

    [Fact]
    public void GameAnimationSampleTic_AddsOne()
    {
        Assert.Equal(5, VmActorPrecalcAnimation.GameAnimationSampleTic(4));
    }

    [Fact]
    public void ShouldWriteFindAnimationRetSlot_MatchesNativeNumret()
    {
        Assert.True(VmActorPrecalcAnimation.ShouldWriteFindAnimationRetSlot(3, 2));
        Assert.False(VmActorPrecalcAnimation.ShouldWriteFindAnimationRetSlot(1, 2));
    }
}
