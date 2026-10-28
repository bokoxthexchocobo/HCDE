namespace HCDE.Playsim.Tests;

public class VmGGameHead995SliceTests
{
    [Fact]
    public void SpyStepAndAdvance()
    {
        Assert.Equal(1, VmGGameHead995Slice.ResolveSpyStep(VmGGameHead995Slice.SpyNext));
        Assert.Equal(-1, VmGGameHead995Slice.ResolveSpyStep(VmGGameHead995Slice.SpyPrev));
        Assert.True(VmGGameHead995Slice.ShouldAdvanceSpyCamera(VmGGameHead995Slice.SpyNext));

        var next = VmGGameHead995Slice.AdvanceSpyPlayerIndex(
            current: 1,
            spyCommand: VmGGameHead995Slice.SpyNext,
            maxPlayers: 4,
            isEligible: p => p == 2);
        Assert.Equal(2, next);
    }

    [Fact]
    public void AnalogLookAndSlowTurn()
    {
        var (pitch, yaw) = VmGGameHead995Slice.BuildAnalogLookDeltas(
            axisPitch: 0.5f,
            axisYaw: -0.25f,
            pitchSensitivity: 1f,
            yawSensitivity: 1f);
        Assert.Equal(640, pitch);
        Assert.Equal(320, yaw);
        Assert.Equal(4, VmGGameHead995Slice.ApplySlowTurnSpeedBoost(turnHeld: 2, turnSpeed: 1));
    }

    [Fact]
    public void TurboForwardMoveGate()
    {
        Assert.True(VmGGameHead995Slice.ForwardMoveExceedsTurboThreshold(12801));
        Assert.True(VmGGameHead995Slice.ShouldDetectTurboForwardMove(
            multiplayer: true,
            turboPercent: 101f,
            forwardMove: 12801));
    }
}
