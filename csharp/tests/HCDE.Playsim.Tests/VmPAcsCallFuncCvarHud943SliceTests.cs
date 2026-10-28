namespace HCDE.Playsim.Tests;

public class VmPAcsCallFuncCvarHud943SliceTests
{
    [Fact]
    public void MinArgCountMet_RequiresAtLeastN() =>
        Assert.False(VmPAcsCallFuncCvarHud943Slice.MinArgCountMet(1, 2));

    [Fact]
    public void MapLineAttackFlags_TranslatesFhfBits()
    {
        var mapped = VmPAcsCallFuncCvarHud943Slice.MapLineAttackFlags(
            VmPAcsCallFuncCvarHud943Slice.FhfNoRandomPuffZ | VmPAcsCallFuncCvarHud943Slice.FhfNoImpactDecal);
        Assert.Equal(
            VmPAcsCallFuncCvarHud943Slice.LafNoRandomPuffZ | VmPAcsCallFuncCvarHud943Slice.LafNoImpactDecal,
            mapped);
    }

    [Fact]
    public void LineAttackUsesActivator_WhenTidZero() =>
        Assert.True(VmPAcsCallFuncCvarHud943Slice.LineAttackUsesActivator(0));

    [Fact]
    public void ComposePlayChannel_AddsLocalAndLoopFlags()
    {
        var chan = VmPAcsCallFuncCvarHud943Slice.ComposePlayChannel(3, true, true);
        Assert.Equal(3, VmPAcsCallFuncCvarHud943Slice.PlaySoundChannelMask(chan));
        Assert.NotEqual(0, chan & VmPAcsCallFuncCvarHud943Slice.ChanfLocal);
    }

    [Fact]
    public void ResolvePlaySoundSpot_ReturnsActivatorForTidZero()
    {
        var activator = new object();
        Assert.Same(activator, VmPAcsCallFuncCvarHud943Slice.ResolvePlaySoundSpot(0, activator, new object()));
    }

    [Fact]
    public void DefaultStopChannel_FallsBackToBody() =>
        Assert.Equal(VmPAcsCallFuncCvarHud943Slice.ChanBody, VmPAcsCallFuncCvarHud943Slice.DefaultStopChannel(1, 9));

    [Fact]
    public void SetUserCVarRequiresThreeArgs_MatchesNative() =>
        Assert.True(VmPAcsCallFuncCvarHud943Slice.SetUserCVarRequiresThreeArgs(3));
}
