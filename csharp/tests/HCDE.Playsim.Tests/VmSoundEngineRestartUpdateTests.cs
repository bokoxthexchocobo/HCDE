namespace HCDE.Playsim.Tests;

public class VmSoundEngineRestartUpdateTests
{
    [Fact]
    public void RequiresEvictedFlag_DetectsRestart() =>
        Assert.True(VmSoundEngineRestartUpdate.RequiresEvictedFlag(VmSoundEngineRestartUpdate.ChanfEvicted));

    [Fact]
    public void BuildStartFlags_CombinesLoopAndArea()
    {
        var flags = VmSoundEngineRestartUpdate.BuildStartFlags(
            chanFlags: VmSoundEngineRestartUpdate.ChanfLoop | 4,
            chanfLoop: VmSoundEngineRestartUpdate.ChanfLoop,
            chanfArea: 4,
            chanfUi: 8,
            chanfNopause: 16,
            chanfAbstime: 32);
        Assert.Equal(1 | 2, flags);
    }

    [Fact]
    public void AbortRestartNearLimit_BlocksWhenLimited() =>
        Assert.True(VmSoundEngineRestartUpdate.AbortRestartNearLimit(2, limitHit: true));

    [Fact]
    public void ClampChannelVolume_ClipsToUnitInterval()
    {
        Assert.Equal(0f, VmSoundEngineRestartUpdate.ClampChannelVolume(-0.1));
        Assert.Equal(1f, VmSoundEngineRestartUpdate.ClampChannelVolume(2));
    }

    [Fact]
    public void ShouldChangeChannelVolume_MatchesActorAndChannel()
    {
        var actor = new object();
        Assert.True(VmSoundEngineRestartUpdate.ShouldChangeChannelVolume(1, 1, actor, actor, 0, -1));
        Assert.False(VmSoundEngineRestartUpdate.ShouldChangeChannelVolume(1, 1, actor, actor, 0, 1));
    }

    [Fact]
    public void ClearEvictedFlags_ClearsBits()
    {
        const int evicted = 1;
        const int abstime = 2;
        var cleared = VmSoundEngineRestartUpdate.ClearEvictedFlags(0b111, evicted, abstime);
        Assert.Equal(0b100, cleared);
    }
}
