namespace HCDE.Playsim.Tests;

public class VmPTicker977SliceTests
{
    [Fact]
    public void RecreateLightsFlag_ClearsWhenSet()
    {
        var flags = VmPTicker977Slice.Mf8RecreateLights;
        Assert.True(VmPTicker977Slice.ActorNeedsRecreateLights(flags));
        Assert.Equal(0, VmPTicker977Slice.ApplyRecreateLightsClear(flags));
    }

    [Fact]
    public void GlobalFreeze_TogglesOnQuarterTics()
    {
        Assert.True(VmPTicker977Slice.ShouldRunGlobalFreezeTick(0));
        Assert.False(VmPTicker977Slice.ShouldRunGlobalFreezeTick(1));

        Assert.True(VmPTicker977Slice.TryApplyGlobalFreezeChange(
            maptime: 4,
            globalChangeFreezePending: true,
            globalFreeze: 0,
            out var next,
            out var applied));
        Assert.True(applied);
        Assert.Equal(1, next);

        var merged = VmPTicker977Slice.MergeGlobalFreezeIntoLevelState(frozenState: 0, globalFreeze: 1);
        Assert.Equal(VmPTicker977Slice.LevelFrozenStateGlobalBit, merged);
    }

    [Fact]
    public void TimeFreezerScan_ResumesWhenAllClear()
    {
        Span<int> freezers = [0, 1, 0];
        Span<bool> inGame = [true, true, false];
        Assert.False(VmPTicker977Slice.AllPlayersClearOfTimeFreezer(freezers, inGame));

        freezers[1] = 0;
        Assert.True(VmPTicker977Slice.AllPlayersClearOfTimeFreezer(freezers, inGame));
    }

    [Fact]
    public void TickerPauseAndInterpolation_Gates()
    {
        Assert.True(VmPTicker977Slice.TickerPausedEarlyOut(paused: true, checkTickerPaused: false));
        Assert.True(VmPTicker977Slice.RendererInterpolationAfterSightReset(false, false));
        Assert.False(VmPTicker977Slice.RendererInterpolationAfterSightReset(true, false));
    }

    [Fact]
    public void HcdeJoiner_SkipsPlayerThinkUntilAuthority()
    {
        Assert.True(VmPTicker977Slice.SkipHcdeRuntimeJoinerPlayerThink(
            mobPresent: false,
            netGame: true,
            clientUsesHcdeService: true,
            isAuthoritySlot: false));
        Assert.False(VmPTicker977Slice.ShouldInvokePlayerThink(
            playerInLevel: true,
            skipHcdeJoiner: true));
        Assert.True(VmPTicker977Slice.ShouldInvokePlayerThink(true, false));
    }

    [Fact]
    public void LevelClocksAndSpecials()
    {
        Assert.True(VmPTicker977Slice.ShouldUpdateSpecials(levelFrozen: false));
        Assert.False(VmPTicker977Slice.ShouldUpdateSpecials(true));
        var clocks = VmPTicker977Slice.IncrementLevelClocks(1, 2, 3);
        Assert.Equal((2, 3, 4), clocks);
        Assert.True(VmPTicker977Slice.ShouldClearStatusBarInterpolation(new object()));
    }
}
