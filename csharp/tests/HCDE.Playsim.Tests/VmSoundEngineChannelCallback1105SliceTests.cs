namespace HCDE.Playsim.Tests;

public class VmSoundEngineChannelCallback1105SliceTests
{
    [Fact]
    public void ResolveChannelEndedEvicted_TreatsLoopAsEvicted()
    {
        var evicted = VmSoundEngineChannelCallback1105Slice.ResolveChannelEndedEvicted(
            new VmSoundEngineChannelCallback1105Slice.ChannelEndedContext
            {
                Looping = true,
                ChanFlags = 0
            });
        Assert.True(evicted);
    }

    [Fact]
    public void ApplyVirtualFlag_SetsAndClearsBit()
    {
        var withVirtual = VmSoundEngineChannelCallback1105Slice.ApplyVirtualFlag(0, true);
        Assert.True((withVirtual & VmSoundEngineChannelCallback1105Slice.ChanfVirtual) != 0);
        var cleared = VmSoundEngineChannelCallback1105Slice.ApplyVirtualFlag(withVirtual, false);
        Assert.Equal(0, cleared & VmSoundEngineChannelCallback1105Slice.ChanfVirtual);
    }

    [Fact]
    public void RunStopChannel_ReturnsWhenNoSysChannel()
    {
        var returned = false;
        VmSoundEngineChannelCallback1105Slice.RunStopChannel(
            new VmSoundEngineChannelCallback1105Slice.StopChannelContext
            {
                HasSysChannel = false,
                ReturnChannel = () => returned = true
            });
        Assert.True(returned);
    }

    [Fact]
    public void CountUnloadIndices_CountsPredicateMatches()
    {
        var count = VmSoundEngineChannelCallback1105Slice.CountUnloadIndices(4, i => i % 2 == 0);
        Assert.Equal(2, count);
    }
}
