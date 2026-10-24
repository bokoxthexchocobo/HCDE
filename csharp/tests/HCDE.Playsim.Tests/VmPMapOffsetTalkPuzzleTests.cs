namespace HCDE.Playsim.Tests;

public class VmPMapOffsetTalkPuzzleTests
{
    [Fact]
    public void OffsetView_OobHardwareSkipsTrace() =>
        Assert.True(VmPMapOffsetTalkPuzzle.AllowOutOfBandOffset(true, true));

    [Fact]
    public void OffsetView_PortalWalkWhenNotOobHw() =>
        Assert.True(VmPMapOffsetTalkPuzzle.ShouldPortalWalk(false, true));

    [Fact]
    public void OffsetView_FloorPortalDown() =>
        Assert.True(VmPMapOffsetTalkPuzzle.WalkFloorPortalDown(-1, false, 0, 10));

    [Fact]
    public void TalkFacing_ThreeAngleOffsets() =>
        Assert.Equal(3, VmPMapOffsetTalkPuzzle.TalkFacingAngles().Length);

    [Fact]
    public void TalkFacing_ConversationFlags() =>
        Assert.NotEqual(0, VmPMapOffsetTalkPuzzle.AimLineAttackFlags() & VmPMapOffsetTalkPuzzle.AlfCheckconversation);

    [Fact]
    public void PuzzleLine_MatchingType() =>
        Assert.True(VmPMapOffsetTalkPuzzle.TryUsePuzzleOnLine(
            new VmPMapOffsetTalkPuzzle.PuzzleLineUseContext(
                VmPMapOffsetTalkPuzzle.UsePuzzleItemSpecial, 32, 0, 3, 3)));

    [Fact]
    public void PuzzleLine_WrongSide() =>
        Assert.False(VmPMapOffsetTalkPuzzle.TryUsePuzzleOnLine(
            new VmPMapOffsetTalkPuzzle.PuzzleLineUseContext(
                VmPMapOffsetTalkPuzzle.UsePuzzleItemSpecial, 32, 1, 3, 3)));

    [Fact]
    public void PuzzleThing_MatchingType() =>
        Assert.True(VmPMapOffsetTalkPuzzle.TryUsePuzzleOnThing(
            new VmPMapOffsetTalkPuzzle.PuzzleThingUseContext(
                VmPMapOffsetTalkPuzzle.UsePuzzleItemSpecial, 2, 2)));

    [Fact]
    public void SplashFactor_ClampNonPositive() =>
        Assert.Equal(1f, VmPMapOffsetTalkPuzzle.NormalizeSplashFactor(0f));

    [Fact]
    public void SplashFactor_InvertsForThrust() =>
        Assert.Equal(0.5f, VmPMapOffsetTalkPuzzle.SelfThrustScaleFromSplash(2f));
}
