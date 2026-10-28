namespace HCDE.Playsim.Tests;

public class VmPAcsVmOpcode954SliceTests
{
    [Fact]
    public void ResolveHudSize_NegatesHeightWhenCoveringStatusBar()
    {
        var (w, h) = VmPAcsVmOpcode954Slice.ResolveHudSize(-320, 200, 1);
        Assert.Equal(320, w);
        Assert.Equal(-200, h);
    }

    [Fact]
    public void ResolveLevelInfoValue_ReturnsParTime()
    {
        var level = new VmPAcsVmOpcode954Slice.LevelInfoSnapshot(
            100, 0, 0, 1, 0, 0, 0, 0, 0, 0);
        Assert.Equal(100, VmPAcsVmOpcode954Slice.ResolveLevelInfoValue(
            VmPAcsVmOpcode954Slice.LevelInfoParTime, level));
    }

    [Fact]
    public void ResolveLevelInfoValue_UnknownQueryIsZero()
    {
        var level = new VmPAcsVmOpcode954Slice.LevelInfoSnapshot(
            1, 2, 3, 4, 5, 6, 7, 8, 9, 10);
        Assert.Equal(0, VmPAcsVmOpcode954Slice.ResolveLevelInfoValue(99, level));
    }

    [Fact]
    public void SkyNameApplies_RequiresNonEmptyFirstChar()
    {
        Assert.False(VmPAcsVmOpcode954Slice.SkyNameApplies("".AsSpan()));
        Assert.True(VmPAcsVmOpcode954Slice.SkyNameApplies("SKY1".AsSpan()));
    }

    [Fact]
    public void SetActorStateTargetsActivator_WhenTidZero()
    {
        Assert.True(VmPAcsVmOpcode954Slice.SetActorStateTargetsActivator(0));
        Assert.False(VmPAcsVmOpcode954Slice.SetActorStateTargetsActivator(5));
    }

    [Fact]
    public void AccumulateSetActorStateCount_IncrementsOnSuccess()
    {
        Assert.Equal(2, VmPAcsVmOpcode954Slice.AccumulateSetActorStateCount(1, true));
        Assert.Equal(1, VmPAcsVmOpcode954Slice.AccumulateSetActorStateCount(1, false));
    }

    [Fact]
    public void ResolvePlayerClass_ReturnsNegativeOneWhenUnresolved()
    {
        Assert.Equal(-1, VmPAcsVmOpcode954Slice.ResolvePlayerClass(false, 3));
        Assert.Equal(3, VmPAcsVmOpcode954Slice.ResolvePlayerClass(true, 3));
    }

    [Fact]
    public void AimDistToAcs_ScalesByNativeFactor()
    {
        Assert.Equal((int)(45.0 * VmPAcsVmOpcode954Slice.AimDistBamScale),
            VmPAcsVmOpcode954Slice.AimDistToAcs(45.0));
    }

    [Fact]
    public void UseActivatorAsCamera_WhenTidZero()
    {
        Assert.True(VmPAcsVmOpcode954Slice.UseActivatorAsCamera(0));
    }
}
