namespace HCDE.Playsim.Tests;

public class VmSoundEngineRestartUpdateTests
{
    [Fact]
    public void RequiresEvictedFlag_DetectsRestart() =>
        Assert.True(VmSoundEngineRestartUpdate.RequiresEvictedFlag(VmSoundEngineRestartUpdate.ChanfEvicted));
}

public class VmSoundEngineUpdateSoundsTests
{
    [Fact]
    public void UpdateChannelWhenOpen_RespectsLimit() =>
        Assert.False(VmSoundEngineUpdateSounds.UpdateChannelWhenOpen(4, 5));
}
