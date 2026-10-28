namespace HCDE.Playsim.Tests;

public class VmSoundEngineRestartChannelTests
{
    [Fact]
    public void RestartChannel_AbortsWithoutEvictedFlag()
    {
        var ctx = new VmSoundEngineRestartChannel.RestartChannelContext { ChanFlags = 0 };
        Assert.False(VmSoundEngineRestartChannel.RestartChannel(ctx));
    }

    [Fact]
    public void RestartChannel_ClearsEvictedOnSuccess()
    {
        var ctx = new VmSoundEngineRestartChannel.RestartChannelContext
        {
            ChanFlags = VmSoundEngineRestartChannel.ChanfEvicted | VmSoundEngineRestartChannel.ChanfLoop,
            SoundId = 1,
            Lumpnum = 1,
            LoadSound = _ => 1,
            StartSound2D = _ => new object(),
        };
        Assert.True(VmSoundEngineRestartChannel.RestartChannel(ctx));
        Assert.Equal(0, ctx.ChanFlags & VmSoundEngineRestartChannel.ChanfEvicted);
    }
}
