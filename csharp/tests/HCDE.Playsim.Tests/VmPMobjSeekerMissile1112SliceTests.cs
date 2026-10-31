namespace HCDE.Playsim.Tests;

public class VmPMobjSeekerMissile1112SliceTests
{
    [Fact]
    public void ShouldZeroMbfVerticalSpeed_WhenBelowMassGravity()
    {
        Assert.True(VmPMobjSeekerMissile1112Slice.ShouldZeroMbfVerticalSpeed(0.1, 64, 1));
        Assert.False(VmPMobjSeekerMissile1112Slice.ShouldZeroMbfVerticalSpeed(2, 64, 1));
    }

    [Fact]
    public void FaceMobjDirection_ClockwiseWhenDeltaPositive()
    {
        Assert.Equal(1, VmPMobjSeekerMissile1112Slice.FaceMobjDirection(15));
        Assert.Equal(0, VmPMobjSeekerMissile1112Slice.FaceMobjDirection(-4));
        Assert.Equal(4, VmPMobjSeekerMissile1112Slice.FaceMobjTurnAmount(-4));
    }

    [Fact]
    public void CanSeek_RejectsInvisibleWhenFlagged()
    {
        var ok = VmPMobjSeekerMissile1112Slice.CanSeek(new VmPMobjSeekerMissile1112Slice.CanSeekContext
        {
            SeekerFlags2 = VmPMobjSeekerMissile1112Slice.Mf2DontSeekInvisible,
            TargetRenderFlags = VmPMobjSeekerMissile1112Slice.RfInvisible,
            RenderStyleVisible = true
        });
        Assert.False(ok);
    }

    [Fact]
    public void RunSeekerMissile_HalvesDeltaAboveThreshold()
    {
        var ctx = new VmPMobjSeekerMissile1112Slice.SeekerTurnContext
        {
            HasTarget = true,
            CanSeekTarget = true,
            TargetShootable = true,
            ActorSpeed = 8,
            Delta = 40,
            FaceDirection = 1,
            Threshold = 10,
            TurnMax = 12
        };
        Assert.True(VmPMobjSeekerMissile1112Slice.RunSeekerMissile(ctx));
        Assert.Equal(12, ctx.Delta);
        Assert.Equal(12, ctx.Yaw);
    }

    [Fact]
    public void PreciseAimHeight_UsesViewZForPlayers()
    {
        Assert.Equal(32, VmPMobjSeekerMissile1112Slice.PreciseAimHeight(true, 56, 72, 40));
        Assert.Equal(28, VmPMobjSeekerMissile1112Slice.PreciseAimHeight(false, 56, 72, 40));
    }
}
