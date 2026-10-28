namespace HCDE.Playsim.Tests;

public class VmPUserDeathIdleTests
{
    [Fact]
    public void PlayIdle_RequiresVirtual() =>
        Assert.True(VmPUserDeathIdle.ShouldCallPlayIdleVirtual(true, true));

    [Fact]
    public void PlayerScream_WimpyBranch()
    {
        var plan = VmPUserDeathIdle.PlanPlayerScream(new VmPUserDeathIdle.PlayerScreamContext(
            true, false, 0, 0, 0, 5, 0,
            (_, _) => "wimpy",
            _ => null));
        Assert.Equal("wimpy", plan.SoundId);
        Assert.Equal(VmPUserDeathIdle.ChanVoice, plan.Channel);
    }

    [Fact]
    public void PlayerScream_SplatUsesBodyChannel()
    {
        var plan = VmPUserDeathIdle.PlanPlayerScream(new VmPUserDeathIdle.PlayerScreamContext(
            true, false, 0, VmPUserDeathIdle.DfForceFallingHx, -40, 0, 0,
            (_, _) => null,
            _ => "splat"));
        Assert.Equal(VmPUserDeathIdle.ChanBody, plan.Channel);
        Assert.True(plan.StopAuxChannels);
    }

    [Fact]
    public void PlayerScream_ConfiguredDeathBypassesVariants()
    {
        var plan = VmPUserDeathIdle.PlanPlayerScream(new VmPUserDeathIdle.PlayerScreamContext(
            false, true, 0, 0, 0, 0, 0,
            (_, _) => "x",
            _ => "x"));
        Assert.Equal("configured", plan.SoundId);
        Assert.False(plan.StopAuxChannels);
    }

    [Fact]
    public void AuxChannelsToStop_SkipsWeaponAndVoice()
    {
        var stops = VmPUserDeathIdle.AuxChannelsToStop(VmPUserDeathIdle.ChanBody).ToArray();
        Assert.Contains(0, stops);
        Assert.DoesNotContain(1, stops);
        Assert.DoesNotContain(2, stops);
    }
}
