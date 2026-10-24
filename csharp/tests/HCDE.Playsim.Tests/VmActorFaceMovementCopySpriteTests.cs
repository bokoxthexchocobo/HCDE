namespace HCDE.Playsim.Tests;

public class VmActorFaceMovementCopySpriteTests
{
    [Fact]
    public void EarlyFaceMovementFail_BothNoPitchAndNoAngle() =>
        Assert.True(VmActorFaceMovementCopySprite.EarlyFaceMovementFail(
            true,
            VmActorFaceMovementCopySprite.FmdfFlags.NoPitch | VmActorFaceMovementCopySprite.FmdfFlags.NoAngle));

    [Fact]
    public void EarlyFaceMovementFail_NoMobj() =>
        Assert.True(VmActorFaceMovementCopySprite.EarlyFaceMovementFail(false, default));

    [Fact]
    public void ClampYawDelta_RespectsLimit() =>
        Assert.Equal(-45, VmActorFaceMovementCopySprite.ClampYawDelta(0, 90, 45, 0));

    [Fact]
    public void ClampYawDelta_WithinLimitUsesTarget() =>
        Assert.Equal(10, VmActorFaceMovementCopySprite.ClampYawDelta(0, 10, 45, 0));

    [Fact]
    public void ClampYawDelta_NoLimitUsesTargetPlusOffset() =>
        Assert.Equal(15, VmActorFaceMovementCopySprite.ClampYawDelta(0, 10, 0, 5));

    [Fact]
    public void ClampPitchDelta_WhenOverLimit() =>
        Assert.Equal(-20, VmActorFaceMovementCopySprite.ClampPitchDelta(0, 50, 20));

    [Fact]
    public void PitchFromVelocity_MatchesAtan() =>
        Assert.Equal(0, VmActorFaceMovementCopySprite.PitchFromVelocity(1, 0, 0));

    [Fact]
    public void ShouldAdjustYaw_WhenMoving() =>
        Assert.True(VmActorFaceMovementCopySprite.ShouldAdjustYaw(default, 1, 0));

    [Fact]
    public void FaceMovementPitch_AdjustsViaClamp()
    {
        var target = VmActorFaceMovementCopySprite.PitchFromVelocity(0, 1, 1);
        var pitch = VmActorFaceMovementCopySprite.ClampPitchDelta(0, target, 30);
        Assert.NotEqual(target, pitch);
    }

    [Fact]
    public void CopySpriteFrameShouldFail_WhenBothSkipFlags() =>
        Assert.True(VmActorFaceMovementCopySprite.CopySpriteFrameShouldFail(
            false, true, true,
            VmActorFaceMovementCopySprite.CpsfFlags.NoSprite | VmActorFaceMovementCopySprite.CpsfFlags.NoFrame));

    [Fact]
    public void SetVisibleRotationShouldFail_WithoutMobj() =>
        Assert.True(VmActorFaceMovementCopySprite.SetVisibleRotationShouldFail(false));

    [Fact]
    public void ShouldAssignVisibleStartAngle_WhenFlagClear() =>
        Assert.True(VmActorFaceMovementCopySprite.ShouldAssignVisibleStartAngle(default));

    [Fact]
    public void SetTranslationRequiresName_WhenPresent() =>
        Assert.True(VmActorFaceMovementCopySprite.SetTranslationRequiresName(true));
}
