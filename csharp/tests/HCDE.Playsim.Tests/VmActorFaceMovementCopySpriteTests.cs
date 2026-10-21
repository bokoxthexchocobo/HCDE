namespace HCDE.Playsim.Tests;

public class VmActorFaceMovementCopySpriteTests
{
    [Fact]
    public void EarlyFaceMovementFail_BothNoPitchAndNoAngle() =>
        Assert.True(VmActorFaceMovementCopySprite.EarlyFaceMovementFail(
            true,
            VmActorFaceMovementCopySprite.FmdfFlags.NoPitch | VmActorFaceMovementCopySprite.FmdfFlags.NoAngle));

    [Fact]
    public void ClampYawDelta_RespectsLimit() =>
        Assert.Equal(-45, VmActorFaceMovementCopySprite.ClampYawDelta(0, 90, 45, 0));

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
